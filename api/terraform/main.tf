terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
  }
  backend "s3" {
    bucket = "galaxy-terraform-prod-af-south-1"
    key    = "api/terraform/galaxy-api.tfstate"
    region = "af-south-1"
  }
}

provider "aws" {
  region = "af-south-1"
}

# Random suffix for unique resource names
resource "random_id" "suffix" {
  byte_length = 4
}

# Define variables
variable "environment" {
  description = "The environment (dev, test, prod)"
  default     = "dev"
}

variable "app_name" {
  description = "The name of the application"
  default     = "galaxy-match-make"
}

# Create VPC and networking infrastructure
resource "aws_vpc" "api_vpc" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = {
    Name = "${var.app_name}-vpc-${var.environment}-${random_id.suffix.hex}"
  }
}

resource "aws_subnet" "public_subnet_1" {
  vpc_id            = aws_vpc.api_vpc.id
  cidr_block        = "10.0.1.0/24"
  availability_zone = "af-south-1a"

  tags = {
    Name = "${var.app_name}-public-subnet-1-${var.environment}-${random_id.suffix.hex}"
  }
}

resource "aws_subnet" "public_subnet_2" {
  vpc_id            = aws_vpc.api_vpc.id
  cidr_block        = "10.0.2.0/24"
  availability_zone = "af-south-1b"

  tags = {
    Name = "${var.app_name}-public-subnet-2-${var.environment}-${random_id.suffix.hex}"
  }
}

resource "aws_internet_gateway" "igw" {
  vpc_id = aws_vpc.api_vpc.id

  tags = {
    Name = "${var.app_name}-igw-${var.environment}"
  }
}

resource "aws_route_table" "public_rt" {
  vpc_id = aws_vpc.api_vpc.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id
  }

  tags = {
    Name = "${var.app_name}-public-rt-${var.environment}"
  }
}

resource "aws_route_table_association" "public_subnet_1_association" {
  subnet_id      = aws_subnet.public_subnet_1.id
  route_table_id = aws_route_table.public_rt.id
}

resource "aws_route_table_association" "public_subnet_2_association" {
  subnet_id      = aws_subnet.public_subnet_2.id
  route_table_id = aws_route_table.public_rt.id
}

# Security Group for API
resource "aws_security_group" "api_sg" {
  name        = "${var.app_name}-api-sg-${var.environment}-${random_id.suffix.hex}"
  description = "Security group for API"
  vpc_id      = aws_vpc.api_vpc.id

  ingress {
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "${var.app_name}-api-sg-${var.environment}-${random_id.suffix.hex}"
  }
}

# ECR Repository
resource "aws_ecr_repository" "api_ecr" {
  name                 = "${var.app_name}-api-${var.environment}-${random_id.suffix.hex}"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name = "${var.app_name}-ecr-${var.environment}-${random_id.suffix.hex}"
  }
}

# EC2 Instance Profile Role
resource "aws_iam_role" "ec2_role" {
  name = "${var.app_name}-ec2-role-${var.environment}-${random_id.suffix.hex}"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect = "Allow"
        Principal = {
          Service = "ec2.amazonaws.com"
        }
      }
    ]
  })

  tags = {
    Name = "${var.app_name}-ec2-role-${var.environment}-${random_id.suffix.hex}"
  }
}

# Attach policies to the EC2 role
resource "aws_iam_role_policy_attachment" "ecr_read_policy" {
  role       = aws_iam_role.ec2_role.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonECR-FullAccess"
}

resource "aws_iam_role_policy_attachment" "secrets_manager_policy" {
  role       = aws_iam_role.ec2_role.name
  policy_arn = "arn:aws:iam::aws:policy/SecretsManagerReadWrite"
}

# Create instance profile from role
resource "aws_iam_instance_profile" "ec2_profile" {
  name = "${var.app_name}-ec2-profile-${var.environment}-${random_id.suffix.hex}"
  role = aws_iam_role.ec2_role.name
}

# AWS Secrets Manager for storing configuration
resource "aws_secretsmanager_secret" "db_connection" {
  name        = "ConnectionStrings/DefaultConnection-${var.environment}-${random_id.suffix.hex}"
  description = "Database connection string for ${var.app_name}"

  recovery_window_in_days = 0  # Set to 0 for easier testing, use 7+ for production

  tags = {
    Environment = var.environment
    Application = var.app_name
  }
}

resource "aws_secretsmanager_secret" "google_client_id" {
  name        = "Google/ClientId-${var.environment}-${random_id.suffix.hex}"
  description = "Google OAuth Client ID for ${var.app_name}"

  recovery_window_in_days = 0  # Set to 0 for easier testing, use 7+ for production

  tags = {
    Environment = var.environment
    Application = var.app_name
  }
}

resource "aws_secretsmanager_secret" "google_client_secret" {
  name        = "Google/ClientSecret-${var.environment}-${random_id.suffix.hex}"
  description = "Google OAuth Client Secret for ${var.app_name}"

  recovery_window_in_days = 0  # Set to 0 for easier testing, use 7+ for production

  tags = {
    Environment = var.environment
    Application = var.app_name
  }
}

resource "aws_secretsmanager_secret" "google_redirect_uri" {
  name        = "Google/RedirectUri-${var.environment}-${random_id.suffix.hex}"
  description = "Google OAuth Redirect URI for ${var.app_name}"

  recovery_window_in_days = 0  # Set to 0 for easier testing, use 7+ for production

  tags = {
    Environment = var.environment
    Application = var.app_name
  }
}

# EC2 Instance for API
resource "aws_instance" "api_instance" {
  ami                    = "ami-0bbb14f724a8621e0"  # Amazon Linux 2023 in af-south-1
  instance_type          = "t3.small"
  subnet_id              = aws_subnet.public_subnet_1.id
  vpc_security_group_ids = [aws_security_group.api_sg.id]
  iam_instance_profile   = aws_iam_instance_profile.ec2_profile.name
  associate_public_ip_address = true
  
  user_data = <<-EOF
              #!/bin/bash
              # Update and install Docker
              dnf update -y
              dnf install -y docker
              systemctl start docker
              systemctl enable docker
              
              # Install AWS CLI
              dnf install -y unzip
              curl "https://awscli.amazonaws.com/awscli-exe-linux-x86_64.zip" -o "awscliv2.zip"
              unzip awscliv2.zip
              ./aws/install
              
              # Set environment variables
              echo "ASPNETCORE_ENVIRONMENT=Production" >> /etc/environment
              echo "AWS_REGION=af-south-1" >> /etc/environment
              echo "AWS_SECRET_PREFIX=${random_id.suffix.hex}" >> /etc/environment
              
              # Reload environment variables
              source /etc/environment
              
              # Pull docker image from ECR
              aws ecr get-login-password --region af-south-1 | docker login --username AWS --password-stdin ${aws_ecr_repository.api_ecr.repository_url}
              docker pull ${aws_ecr_repository.api_ecr.repository_url}:latest
              
              # Run the container
              docker run -d -p 80:80 \
                --name galaxy-api \
                -e ASPNETCORE_ENVIRONMENT=Production \
                -e AWS_REGION=af-south-1 \
                -e AWS_SECRET_PREFIX=${random_id.suffix.hex} \
                ${aws_ecr_repository.api_ecr.repository_url}:latest
              EOF

  tags = {
    Name = "${var.app_name}-instance-${var.environment}-${random_id.suffix.hex}"
  }
}

# Outputs
output "api_instance_public_ip" {
  value       = aws_instance.api_instance.public_ip
  description = "Public IP of the API EC2 instance"
}

output "api_instance_public_dns" {
  value       = aws_instance.api_instance.public_dns
  description = "Public DNS of the API EC2 instance"
}

output "ecr_repository_url" {
  value       = aws_ecr_repository.api_ecr.repository_url
  description = "URL of the ECR repository"
}

output "random_suffix" {
  value       = random_id.suffix.hex
  description = "Random suffix used for resource names"
}
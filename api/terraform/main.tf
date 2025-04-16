# Use existing OIDC provider
data "aws_iam_openid_connect_provider" "github" {
  url = "https://token.actions.githubusercontent.com"
}

# Use existing VPC data
data "aws_vpc" "default" {
  default = true
}

# Use existing subnet data
data "aws_subnets" "default" {
  filter {
    name   = "vpc-id"
    values = [data.aws_vpc.default.id]
  }
}

data "aws_subnet" "selected" {
  for_each = toset(data.aws_subnets.default.ids)
  id       = each.value
}

# Use existing key pair data
data "aws_key_pair" "existing" {
  key_name = "csharp-levelup-key"
}

resource "aws_security_group" "csharp_levelup_sg" {
  name_prefix = "csharp-levelup-sg-"
  vpc_id      = data.aws_vpc.default.id

  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    description = "Allow SSH access"
  }

  ingress {
    from_port   = 8080
    to_port     = 8080
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    description = "Allow HTTP traffic on port 8080"
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "CSharpLevelUpSecurityGroup"
  }
}

resource "aws_instance" "csharp_levelup_instance" {
  ami                    = data.aws_ami.amazon_linux_latest.id
  instance_type          = "t3.micro"
  key_name               = data.aws_key_pair.existing.key_name
  vpc_security_group_ids = [aws_security_group.csharp_levelup_sg.id]
  subnet_id              = element([for s in data.aws_subnet.selected : s.id], 0)
  iam_instance_profile   = aws_iam_instance_profile.csharp_levelup_instance_profile.name
  root_block_device {
    volume_size = 24
  }

  user_data = <<-EOF
              #!/bin/bash
              # Update package list
              sudo yum update -y
              # Install Docker
              sudo amazon-linux-extras enable docker
              sudo yum install -y docker
              # Start and enable Docker service
              sudo systemctl start docker
              sudo systemctl enable docker
              # Add EC2 user to Docker group
              sudo usermod -aG docker ec2-user
              # Print Docker version
              docker --version
              EOF

  tags = {
    Name = "CSharpLevelUpEC2"
  }
}

# Rest of your IAM resources remain the same
resource "aws_iam_role" "github_actions" {
  name = "CSharpLevelUpActionsRole-${var.github_org}-${var.repository_name}"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Federated = data.aws_iam_openid_connect_provider.github.arn
        }
        Action = "sts:AssumeRoleWithWebIdentity"
        Condition = {
          StringEquals = {
            "token.actions.githubusercontent.com:aud" = var.oidc_audience
          }
          StringLike = {
            "token.actions.githubusercontent.com:sub" = "repo:${var.github_org}/${var.repository_name}:*"
          }
        }
      },
    ]
  })
  tags = {
    GitHubOrg      = var.github_org
    RepositoryName = var.repository_name
  }
}

resource "aws_iam_policy_attachment" "github_actions_policy_attachment" {
  name       = "CSharpLevelUpActionsPolicyAttachment"
  roles      = [aws_iam_role.github_actions.name]
  policy_arn = "arn:aws:iam::aws:policy/EC2InstanceProfileForImageBuilderECRContainerBuilds"
}

resource "aws_iam_openid_connect_provider" "github" {
  count        = var.oidc_provider_arn == "" ? 1 : 0
  url          = "https://token.actions.githubusercontent.com"
  client_id_list = ["sts.amazonaws.com"]
  thumbprint_list = ["6938fd4d98bab03faadb97b34396831e3780aea1"]
}

resource "aws_vpc" "csharp_levelup_vpc" {
  cidr_block = "10.0.0.0/16"
  tags = {
    Name = "CSharpLevelUpVPC"
  }
}

resource "aws_subnet" "csharp_levelup_subnet" {
  vpc_id            = aws_vpc.csharp_levelup_vpc.id
  cidr_block        = "10.0.1.0/24"
  availability_zone = "af-south-1a"
  map_public_ip_on_launch = true
  tags = {
    Name = "CSharpLevelUpSubnet"
  }
}

resource "aws_internet_gateway" "csharp_levelup_igw" {
  vpc_id = aws_vpc.csharp_levelup_vpc.id
  tags = {
    Name = "CSharpLevelUpIGW"
  }
}

resource "aws_route_table" "csharp_levelup_rt" {
  vpc_id = aws_vpc.csharp_levelup_vpc.id
  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.csharp_levelup_igw.id
  }
  tags = {
    Name = "CSharpLevelUpRouteTable"
  }
}

resource "aws_route_table_association" "csharp_levelup_rta" {
  subnet_id      = aws_subnet.csharp_levelup_subnet.id
  route_table_id = aws_route_table.csharp_levelup_rt.id
}

resource "aws_security_group" "csharp_levelup_sg" {
  name_prefix = "csharp-levelup-sg-"
  vpc_id      = aws_vpc.csharp_levelup_vpc.id

  ingress {
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    description = "Allow SSH access"
  }

  ingress {
    from_port   = 8080
    to_port     = 8080
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
    description = "Allow HTTP traffic on port 8080"
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "CSharpLevelUpSecurityGroup"
  }
}

resource "aws_key_pair" "csharp_levelup_key" {
  key_name   = "csharp-levelup-key"
  public_key = var.ec2_public_key
}

resource "aws_instance" "csharp_levelup_instance" {
  ami                    = data.aws_ami.amazon_linux_latest.id
  instance_type          = "t3.micro"
  key_name               = aws_key_pair.csharp_levelup_key.key_name
  vpc_security_group_ids = [aws_security_group.csharp_levelup_sg.id]
  subnet_id              = aws_subnet.csharp_levelup_subnet.id
  iam_instance_profile   = aws_iam_instance_profile.csharp_levelup_instance_profile.name
  root_block_device {
    volume_size = 24
  }

  user_data = <<-EOF
              #!/bin/bash
              # Update package list
              sudo yum update -y
              # Install Docker
              sudo amazon-linux-extras enable docker
              sudo yum install -y docker
              # Start and enable Docker service
              sudo systemctl start docker
              sudo systemctl enable docker
              # Add EC2 user to Docker group (optional, allows non-root users to run Docker)
              sudo usermod -aG docker ec2-user
              # Print Docker version to verify installation
              docker --version
              EOF

  tags = {
    Name = "CSharpLevelUpEC2"
  }
}

resource "aws_iam_role" "csharp_levelup_instance_role" {
  name_prefix        = "CSharpLevelUpEC2Role-"
  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Action = "sts:AssumeRole"
        Effect   = "Allow"
        Principal = {
          Service = "ec2.amazonaws.com"
        }
      },
    ]
  })

  tags = {
    Name = "CSharpLevelUpEC2Role"
  }
}

resource "aws_iam_role_policy_attachment" "csharp_levelup_instance_role_policy_attachment" {
  role       = aws_iam_role.csharp_levelup_instance_role.name
  policy_arn = "arn:aws:iam::aws:policy/EC2InstanceProfileForImageBuilderECRContainerBuilds"
}

resource "aws_iam_instance_profile" "csharp_levelup_instance_profile" {
  name_prefix = "CSharpLevelUpEC2Profile-"
  role        = aws_iam_role.csharp_levelup_instance_role.name
}

resource "aws_ecr_repository" "csharp_levelup_repo" {
  name               = "csharp-levelup-repo"
  image_tag_mutability = "MUTABLE"
  image_scanning_configuration {
    scan_on_push = false
  }
  tags = {
    Name        = "csharp-levelup-repo"
    Environment = "dev"
  }
}

data "aws_ami" "amazon_linux_latest" {
  most_recent = true
  owners      = ["amazon"]

  filter {
    name   = "name"
    values = ["amzn2-ami-hvm-*-x86_64-gp2"]
  }

  filter {
    name   = "virtualization-type"
    values = ["hvm"]
  }
}

data "aws_region" "current" {}
data "aws_caller_identity" "current" {}

variable "github_org" {
  type        = string
  description = "Name of GitHub organization/user (case sensitive)"
}

variable "repository_name" {
  type        = string
  description = "Name of GitHub repository (case sensitive)"
}

variable "oidc_provider_arn" {
  type        = string
  description = "Arn for the GitHub OIDC Provider."
  default     = ""
}

variable "oidc_audience" {
  type        = string
  description = "Audience supplied to configure-aws-credentials."
  default     = "sts.amazonaws.com"
}

variable "ec2_public_key" {
  type        = string
  description = "Your SSH public key"
}

output "role_arn" {
  value       = aws_iam_role.github_actions.arn
  description = "ARN of the created IAM Role"
}

output "instance_public_ip" {
  value       = aws_instance.csharp_levelup_instance.public_ip
  description = "Public IP address of the EC2 instance"
}

output "ecr_repository_url" {
  value       = aws_ecr_repository.csharp_levelup_repo.repository_url
  description = "URL of the ECR repository"
}
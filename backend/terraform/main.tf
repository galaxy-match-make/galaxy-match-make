terraform {
  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.0"
    }
    postgresql = {
      source  = "cyrilgdn/postgresql"
      version = "~> 1.19.0"
    }
  }

  backend "s3" {
    bucket = "galaxy-terraform-prod-af-south-1"
    key    = "backend/terraform/galaxy-matchdb.tfstate"
    region = "af-south-1"
  }
}

provider "aws" {
  region = "af-south-1"
}

provider "postgresql" {
  host            = aws_db_instance.galaxy-matchdb.address
  port            = 5432
  username        = var.db_username
  password        = var.db_password
  sslmode         = "require"
  connect_timeout = 15
  superuser       = false
}

resource "random_id" "suffix" {
  byte_length = 4
}

data "aws_availability_zones" "available" {}

resource "aws_vpc" "main" {
  cidr_block           = "10.0.0.0/16"
  enable_dns_support   = true
  enable_dns_hostnames = true

  tags = {
    Name = "galaxy-main-vpc-${random_id.suffix.hex}"
  }
}

resource "aws_internet_gateway" "igw" {
  vpc_id = aws_vpc.main.id

  tags = {
    Name = "galaxy-igw-${random_id.suffix.hex}"
  }
}

resource "aws_route_table" "rt" {
  vpc_id = aws_vpc.main.id

  route {
    cidr_block = "0.0.0.0/0"
    gateway_id = aws_internet_gateway.igw.id
  }

  tags = {
    Name = "galaxy-rt-${random_id.suffix.hex}"
  }
}

resource "aws_subnet" "subnet_az1" {
  vpc_id                  = aws_vpc.main.id
  cidr_block              = "10.0.1.0/24"
  availability_zone       = data.aws_availability_zones.available.names[0]
  map_public_ip_on_launch = true

  tags = {
    Name = "subnet-az1-${random_id.suffix.hex}"
  }
}

resource "aws_subnet" "subnet_az2" {
  vpc_id                  = aws_vpc.main.id
  cidr_block              = "10.0.2.0/24"
  availability_zone       = data.aws_availability_zones.available.names[1]
  map_public_ip_on_launch = true

  tags = {
    Name = "subnet-az2-${random_id.suffix.hex}"
  }
}

resource "aws_subnet" "subnet_az3" {
  vpc_id                  = aws_vpc.main.id
  cidr_block              = "10.0.3.0/24"
  availability_zone       = data.aws_availability_zones.available.names[2]
  map_public_ip_on_launch = true

  tags = {
    Name = "subnet-az3-${random_id.suffix.hex}"
  }
}

resource "aws_route_table_association" "a1" {
  subnet_id      = aws_subnet.subnet_az1.id
  route_table_id = aws_route_table.rt.id
}

resource "aws_route_table_association" "a2" {
  subnet_id      = aws_subnet.subnet_az2.id
  route_table_id = aws_route_table.rt.id
}

resource "aws_route_table_association" "a3" {
  subnet_id      = aws_subnet.subnet_az3.id
  route_table_id = aws_route_table.rt.id
}

resource "aws_db_subnet_group" "my_db_subnet_group" {
  name        = "galaxy-match-db-subnet-group-${random_id.suffix.hex}"
  description = "Subnet group for the Galaxy Match database"
  subnet_ids  = [
    aws_subnet.subnet_az1.id,
    aws_subnet.subnet_az2.id,
    aws_subnet.subnet_az3.id
  ]
}

resource "aws_security_group" "db_sg" {
  name        = "galaxy-match-db-sg-${random_id.suffix.hex}"
  description = "Security group for galaxy match make database"
  vpc_id      = aws_vpc.main.id
  
  ingress {
    from_port   = 5432
    to_port     = 5432
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }
}

resource "aws_db_instance" "galaxy-matchdb" {
  allocated_storage    = 20
  storage_type         = "gp2"
  engine               = "postgres"
  engine_version       = "17.4"
  instance_class       = "db.t3.micro"
  identifier           = "galaxy-match-db-${random_id.suffix.hex}"
  username             = var.db_username
  password             = var.db_password
  skip_final_snapshot  = true
  publicly_accessible  = true
  db_subnet_group_name = aws_db_subnet_group.my_db_subnet_group.name
  vpc_security_group_ids = [aws_security_group.db_sg.id]

  depends_on = [aws_route_table_association.a1]
}

resource "postgresql_database" "db" {
  name      = var.db_name
  owner     = var.db_username

  depends_on = [aws_db_instance.galaxy-matchdb]
}

# Generate a dynamic connection string output
output "db_connection_string" {
  value       = "Host=${aws_db_instance.galaxy-matchdb.endpoint};Port=5432;Database=${postgresql_database.db.name};Username=${var.db_username};Password=${var.db_password};Pooling=true;Ssl Mode=Require;"
  description = "PostgreSQL connection string"
  sensitive   = true
}

# Output the PostgreSQL endpoint for use in the unified workflow
output "postgresql_server_endpoint" {
  value       = aws_db_instance.galaxy-matchdb.endpoint
  description = "The endpoint of the PostgreSQL server"
}

output "random_suffix" {
  value       = random_id.suffix.hex
  description = "Random suffix used for resource names"
}

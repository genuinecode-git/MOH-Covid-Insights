terraform {
  required_version = ">= 1.9"

  required_providers {
    aws = {
      source  = "hashicorp/aws"
      version = "~> 5.70"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.6"
    }
    null = {
      source  = "hashicorp/null"
      version = "~> 3.2"
    }
  }
}

provider "aws" {
  region = var.region

  default_tags {
    tags = {
      Project     = "MohCovidInsights"
      Environment = var.environment
      ManagedBy   = "Terraform"
    }
  }
}

# CloudFront certificates must live in us-east-1; aliased for future custom-domain use.
provider "aws" {
  alias  = "us_east_1"
  region = "us-east-1"
}
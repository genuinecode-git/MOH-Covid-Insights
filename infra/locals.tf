locals {
  prefix       = "moh-covid-${var.environment}"
  is_ephemeral = var.environment != "prod"

  azs = slice(data.aws_availability_zones.available.names, 0, 2)

  repo_root = abspath("${path.module}/..")
}

data "aws_availability_zones" "available" {
  state = "available"
}

data "aws_caller_identity" "current" {}

data "aws_region" "current" {}
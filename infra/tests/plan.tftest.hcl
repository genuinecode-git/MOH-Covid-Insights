mock_provider "aws" {
  mock_data "aws_availability_zones" {
    defaults = {
      names = ["ap-southeast-1a", "ap-southeast-1b"]
    }
  }

  mock_data "aws_iam_policy_document" {
    defaults = {
      json = "{\"Version\":\"2012-10-17\",\"Statement\":[]}"
    }
  }

  mock_data "aws_caller_identity" {
    defaults = {
      account_id = "123456789012"
    }
  }
}

mock_provider "random" {}
mock_provider "null" {}

variables {
  environment = "dev"
}

run "database_is_encrypted" {
  command = plan
  assert {
    condition     = aws_rds_cluster.main.storage_encrypted == true
    error_message = "Aurora must be encrypted at rest."
  }
}

run "lambdas_are_arm64" {
  command = plan
  assert {
    condition     = contains(aws_lambda_function.api.architectures, "arm64")
    error_message = "API Lambda must be ARM64."
  }
  assert {
    condition     = contains(aws_lambda_function.ingestion.architectures, "arm64")
    error_message = "Ingestion Lambda must be ARM64."
  }
}

run "web_bucket_is_private" {
  command = plan
  assert {
    condition = alltrue([
      aws_s3_bucket_public_access_block.web.block_public_acls,
      aws_s3_bucket_public_access_block.web.restrict_public_buckets,
    ])
    error_message = "Web bucket must be private behind CloudFront."
  }
}

run "prod_protects_the_database" {
  command = plan
  variables {
    environment = "prod"
  }
  assert {
    condition     = aws_rds_cluster.main.deletion_protection == true
    error_message = "Production database must have deletion protection."
  }
}
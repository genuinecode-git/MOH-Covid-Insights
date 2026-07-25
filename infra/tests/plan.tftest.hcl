variables {
  environment = "dev"
  region      = "ap-southeast-1"
}

run "database_is_encrypted_and_private" {
  command = plan

  assert {
    condition     = aws_rds_cluster.main.storage_encrypted == true
    error_message = "Aurora cluster must be encrypted at rest."
  }

  assert {
    condition     = length(aws_db_subnet_group.main.subnet_ids) == 2
    error_message = "Database must span two isolated subnets."
  }
}

run "lambdas_run_on_arm64" {
  command = plan

  assert {
    condition     = contains(aws_lambda_function.api.architectures, "arm64")
    error_message = "API Lambda must be ARM64 to match the built image."
  }

  assert {
    condition     = contains(aws_lambda_function.ingestion.architectures, "arm64")
    error_message = "Ingestion Lambda must be ARM64 to match the built image."
  }
}

run "database_only_accepts_postgres_from_lambda" {
  command = plan

  assert {
    condition     = aws_security_group_rule.db_from_lambda.from_port == 5432
    error_message = "Database security group must only open the Postgres port."
  }

  assert {
    condition     = aws_security_group_rule.db_from_lambda.cidr_blocks == null
    error_message = "Database must not be reachable from arbitrary CIDR ranges."
  }
}

run "ingestion_is_scheduled_and_alarmed" {
  command = plan

  assert {
    condition     = aws_cloudwatch_event_rule.ingestion.schedule_expression == "cron(0 18 ? * MON *)"
    error_message = "Ingestion must run weekly."
  }

  assert {
    condition     = aws_cloudwatch_metric_alarm.ingestion_failure.threshold == 1
    error_message = "A single ingestion failure must raise the alarm."
  }
}

run "web_bucket_blocks_all_public_access" {
  command = plan

  assert {
    condition = alltrue([
      aws_s3_bucket_public_access_block.web.block_public_acls,
      aws_s3_bucket_public_access_block.web.block_public_policy,
      aws_s3_bucket_public_access_block.web.ignore_public_acls,
      aws_s3_bucket_public_access_block.web.restrict_public_buckets,
    ])
    error_message = "Web bucket must be fully private behind CloudFront."
  }
}

run "prod_retains_data_on_destroy" {
  command = plan

  variables {
    environment = "prod"
  }

  assert {
    condition     = aws_rds_cluster.main.deletion_protection == true
    error_message = "Production database must have deletion protection."
  }

  assert {
    condition     = aws_s3_bucket.web.force_destroy == false
    error_message = "Production bucket must not be force-destroyable."
  }
}
resource "aws_iam_role" "ingestion" {
  name               = "${local.prefix}-ingestion-role"
  assume_role_policy = data.aws_iam_policy_document.lambda_assume.json
}

resource "aws_iam_role_policy_attachment" "ingestion_vpc" {
  role       = aws_iam_role.ingestion.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaVPCAccessExecutionRole"
}

resource "aws_iam_role_policy" "ingestion_secret" {
  name   = "read-db-secret"
  role   = aws_iam_role.ingestion.id
  policy = data.aws_iam_policy_document.read_db_secret.json
}

resource "aws_cloudwatch_log_group" "ingestion" {
  name              = "/aws/lambda/${local.prefix}-ingestion"
  retention_in_days = 30
}

resource "aws_lambda_function" "ingestion" {
  function_name = "${local.prefix}-ingestion"
  role          = aws_iam_role.ingestion.arn
  package_type  = "Image"
  image_uri     = "${aws_ecr_repository.this["ingestion"].repository_url}:latest"
  architectures = ["arm64"]
  memory_size   = 1024
  timeout       = 300

  vpc_config {
    subnet_ids         = module.network.private_subnet_ids
    security_group_ids = [module.network.lambda_security_group_id]
  }

  environment {
    variables = {
      Database__Provider = "Postgres"
      DB_SECRET_ARN      = aws_secretsmanager_secret.db.arn
    }
  }

  depends_on = [null_resource.image, aws_cloudwatch_log_group.ingestion, aws_iam_role_policy_attachment.ingestion_vpc]
}

resource "aws_cloudwatch_event_rule" "ingestion" {
  name                = "${local.prefix}-ingestion-schedule"
  schedule_expression = var.ingestion_schedule
}

resource "aws_cloudwatch_event_target" "ingestion" {
  rule = aws_cloudwatch_event_rule.ingestion.name
  arn  = aws_lambda_function.ingestion.arn
}

resource "aws_lambda_permission" "eventbridge" {
  statement_id  = "AllowEventBridgeInvoke"
  action        = "lambda:InvokeFunction"
  function_name = aws_lambda_function.ingestion.function_name
  principal     = "events.amazonaws.com"
  source_arn    = aws_cloudwatch_event_rule.ingestion.arn
}

resource "aws_cloudwatch_metric_alarm" "ingestion_failure" {
  alarm_name          = "${local.prefix}-ingestion-failed"
  namespace           = "AWS/Lambda"
  metric_name         = "Errors"
  statistic           = "Sum"
  period              = 86400
  evaluation_periods  = 1
  threshold           = 1
  comparison_operator = "GreaterThanOrEqualToThreshold"
  treat_missing_data  = "notBreaching"
  dimensions          = { FunctionName = aws_lambda_function.ingestion.function_name }
}
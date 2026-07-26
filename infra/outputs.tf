output "site_url" {
  value = "https://${aws_cloudfront_distribution.main.domain_name}"
}

output "api_url" {
  value = aws_apigatewayv2_api.main.api_endpoint
}

output "database_endpoint" {
  value = aws_rds_cluster.main.endpoint
}

output "ingestion_function_name" {
  value = aws_lambda_function.ingestion.function_name
}
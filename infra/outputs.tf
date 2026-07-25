output "site_url" {
  description = "Public CloudFront URL"
  value       = "https://${aws_cloudfront_distribution.main.domain_name}"
}

output "api_url" {
  description = "Direct API Gateway endpoint"
  value       = aws_api_gateway_stage.prod.invoke_url
}

output "database_endpoint" {
  description = "Aurora writer endpoint"
  value       = aws_rds_cluster.main.endpoint
}

output "ingestion_function_name" {
  description = "Invoke manually with: aws lambda invoke --function-name <this> /dev/stdout"
  value       = aws_lambda_function.ingestion.function_name
}
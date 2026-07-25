variable "environment" {
  description = "Deployment environment"
  type        = string
  default     = "dev"

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "Environment must be dev, staging, or prod."
  }
}

variable "region" {
  description = "AWS region"
  type        = string
  default     = "ap-southeast-1"
}

variable "vpc_cidr" {
  description = "CIDR block for the VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "db_min_capacity" {
  description = "Aurora Serverless v2 minimum ACUs"
  type        = number
  default     = 0.5
}

variable "db_max_capacity" {
  description = "Aurora Serverless v2 maximum ACUs"
  type        = number
  default     = 2
}

variable "ingestion_schedule" {
  description = "EventBridge cron for dataset ingestion"
  type        = string
  default     = "cron(0 18 ? * MON *)"
}

variable "web_dist_path" {
  description = "Path to the built SPA output"
  type        = string
  default     = "../src/MohCovidInsights.Web/dist"
}
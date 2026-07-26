resource "aws_db_subnet_group" "main" {
  name       = "${local.prefix}-db-subnets"
  subnet_ids = module.network.isolated_subnet_ids
  tags       = { Name = "${local.prefix}-db-subnets" }
}

resource "random_password" "db" {
  length  = 32
  special = false
}

resource "random_password" "readonly_db" {
  length  = 32
  special = false
}

resource "aws_rds_cluster" "main" {
  cluster_identifier = "${local.prefix}-db"
  engine             = "aurora-postgresql"
  engine_mode        = "provisioned"
  engine_version     = "16.4"

  database_name   = "mohcovid"
  master_username = "dbadmin"
  master_password = random_password.db.result

  db_subnet_group_name   = aws_db_subnet_group.main.name
  vpc_security_group_ids = [module.network.database_security_group_id]

  storage_encrypted         = true
  backup_retention_period   = local.is_ephemeral ? 1 : 7
  skip_final_snapshot       = local.is_ephemeral
  final_snapshot_identifier = local.is_ephemeral ? null : "${local.prefix}-final"
  deletion_protection       = !local.is_ephemeral

  serverlessv2_scaling_configuration {
    min_capacity = var.db_min_capacity
    max_capacity = var.db_max_capacity
  }
}

resource "aws_rds_cluster_instance" "writer" {
  identifier         = "${local.prefix}-writer"
  cluster_identifier = aws_rds_cluster.main.id
  instance_class     = "db.serverless"
  engine             = aws_rds_cluster.main.engine
  engine_version     = aws_rds_cluster.main.engine_version
}

resource "aws_secretsmanager_secret" "db" {
  name                    = "${local.prefix}-db-credentials"
  recovery_window_in_days = local.is_ephemeral ? 0 : 7
}

resource "aws_secretsmanager_secret_version" "db" {
  secret_id = aws_secretsmanager_secret.db.id
  secret_string = jsonencode({
    username = aws_rds_cluster.main.master_username
    password = random_password.db.result
    host     = aws_rds_cluster.main.endpoint
    port     = aws_rds_cluster.main.port
    dbname   = aws_rds_cluster.main.database_name
  })
}

resource "aws_secretsmanager_secret" "readonly_db" {
  name                    = "${local.prefix}-db-readonly-credentials"
  recovery_window_in_days = local.is_ephemeral ? 0 : 7
}

resource "aws_secretsmanager_secret_version" "readonly_db" {
  secret_id = aws_secretsmanager_secret.readonly_db.id
  secret_string = jsonencode({
    username = "readonly"
    password = random_password.readonly_db.result
    host     = aws_rds_cluster.main.endpoint
    port     = aws_rds_cluster.main.port
    dbname   = aws_rds_cluster.main.database_name
  })
}

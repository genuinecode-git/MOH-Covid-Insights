locals {
  images = {
    api       = "src/MohCovidInsights.Api/Dockerfile"
    ingestion = "src/MohCovidInsights.Ingestion/Dockerfile"
  }
}

resource "aws_ecr_repository" "this" {
  for_each             = local.images
  name                 = "${local.prefix}-${each.key}"
  image_tag_mutability = "MUTABLE"
  force_delete         = local.is_ephemeral

  image_scanning_configuration {
    scan_on_push = true
  }
}

resource "null_resource" "image" {
  for_each = local.images

  triggers = {
    dockerfile = filesha256("${local.repo_root}/${each.value}")
    repository = aws_ecr_repository.this[each.key].repository_url
  }

  provisioner "local-exec" {
    working_dir = local.repo_root
    command     = <<-EOT
      set -euo pipefail
      aws ecr get-login-password --region ${var.region} \
        | docker login --username AWS --password-stdin ${split("/", aws_ecr_repository.this[each.key].repository_url)[0]}
      docker build --platform linux/arm64 -f ${each.value} \
        -t ${aws_ecr_repository.this[each.key].repository_url}:latest .
      docker push ${aws_ecr_repository.this[each.key].repository_url}:latest
    EOT
  }
}
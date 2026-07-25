locals {
  images = {
    api       = "src/MohCovidInsights.Api/Dockerfile"
    ingestion = "src/MohCovidInsights.Ingestion/Dockerfile"
  }
}

resource "aws_ecr_repository" "this" {
  for_each = local.images

  name                 = "${local.prefix}-${each.key}"
  image_tag_mutability = "MUTABLE"
  force_delete         = local.is_ephemeral

  image_scanning_configuration {
    scan_on_push = true
  }
}

resource "aws_ecr_lifecycle_policy" "this" {
  for_each = aws_ecr_repository.this

  repository = each.value.name

  policy = jsonencode({
    rules = [{
      rulePriority = 1
      description  = "Keep the 5 most recent images"
      selection = {
        tagStatus   = "any"
        countType   = "imageCountMoreThan"
        countNumber = 5
      }
      action = { type = "expire" }
    }]
  })
}

# Terraform has no equivalent of CDK's asset bundling, so image build and push
# is an explicit local-exec step keyed on a hash of the source tree.
resource "null_resource" "image" {
  for_each = local.images

  triggers = {
    dockerfile = filesha256("${local.repo_root}/${each.value}")
    sources = sha256(join("", [
      for f in fileset("${local.repo_root}/src", "MohCovidInsights.{Domain,Application,Infrastructure}/**/*.cs") :
      filesha256("${local.repo_root}/src/${f}")
    ]))
    repository = aws_ecr_repository.this[each.key].repository_url
  }

  provisioner "local-exec" {
    working_dir = local.repo_root
    command     = <<-EOT
      set -euo pipefail
      aws ecr get-login-password --region ${var.region} \
        | docker login --username AWS --password-stdin ${split("/", aws_ecr_repository.this[each.key].repository_url)[0]}
      docker build --platform linux/arm64 \
        -f ${each.value} \
        -t ${aws_ecr_repository.this[each.key].repository_url}:latest .
      docker push ${aws_ecr_repository.this[each.key].repository_url}:latest
    EOT
  }
}
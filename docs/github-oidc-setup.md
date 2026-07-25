# GitHub Actions OIDC setup

One-time manual step. Grants GitHub Actions short-lived AWS credentials
without storing long-lived access keys as repository secrets.

## 1. Create the identity provider

    aws iam create-open-id-connect-provider \
      --url https://token.actions.githubusercontent.com \
      --client-id-list sts.amazonaws.com \
      --thumbprint-list 6938fd4d98bab03faadb97b34396831e3780aea1

## 2. Create the role

Trust policy, replacing ACCOUNT_ID and OWNER/REPO:

    {
      "Version": "2012-10-17",
      "Statement": [{
        "Effect": "Allow",
        "Principal": {
          "Federated": "arn:aws:iam::ACCOUNT_ID:oidc-provider/token.actions.githubusercontent.com"
        },
        "Action": "sts:AssumeRoleWithWebIdentity",
        "Condition": {
          "StringEquals": {
            "token.actions.githubusercontent.com:aud": "sts.amazonaws.com"
          },
          "StringLike": {
            "token.actions.githubusercontent.com:sub": "repo:OWNER/REPO:*"
          }
        }
      }]
    }

Attach a permissions policy scoped to the services this stack uses:
EC2, RDS, Lambda, API Gateway, S3, CloudFront, ECR, Secrets Manager,
CloudWatch, EventBridge, IAM.

## 3. Register the role ARN

Repository Settings → Secrets and variables → Actions → Variables:

    AWS_DEPLOY_ROLE_ARN = arn:aws:iam::ACCOUNT_ID:role/github-actions-moh-covid
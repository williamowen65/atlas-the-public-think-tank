# Infrastructure

This folder will contain Atlas infrastructure as code (IaC): versioned configuration for provisioning and managing the environments and services needed to run the application.

Before adding cloud resources, follow [ADR-0007](../docs/architecture/decisions/ADR-0007-evaluate-azure-sql-with-serverless-backend.md) and its [hosting research](../docs/deployment/CLOUD-HOSTING-RESEARCH.md). Azure SQL Database with a serverless backend is a proposed evaluation direction; the production provider, tiers, and resource configuration remain undecided.


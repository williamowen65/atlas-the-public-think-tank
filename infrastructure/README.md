# Atlas infrastructure

The root of Atlas infrastructure as code (IaC). Runtime application adapters remain in Atlas.Application.

Start with the [Azure resource inventory](PROVISIONED-RESOURCES.md) for a detailed explanation of everything Terraform provisions, and what still needs manual setup. Then use [the deployment runbook](DEPLOYMENT.md) for Terraform commands, Azure deployment and environment parity.

Terraform is organized under [terraform](terraform/) with reusable SQL, App Service and observability modules. The original configuration is under [legacy](../legacy/www.thepublicthinktank.com/terraform/testing/main.tf) for reference.

GitHub workflows validate infrastructure and deploy the API bundled with the frontend. Azure setup and running cloud resources remain separate steps.

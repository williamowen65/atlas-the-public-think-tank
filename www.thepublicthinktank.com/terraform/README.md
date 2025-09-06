# Infrastructure as Code

> This README provides details about how Terraform is configured for this project.

## Docs

* https://developer.hashicorp.com/terraform/tutorials/azure-get-started/azure-build

## Prerequisite Steps

* Terraform CLI downloaded locally
* Signing in to Azure CLI locally
	* Setting up a Service Principal in my Azure account to act on behalf of Terraform
	* The signing in step in also makes it so I don't need to manually reference environemnt variables for the Service Principal
		* If I ever want to script Terraform configurations, I may have to set those env variables.
* Set up a .gitignore for terraform output files.


## Creating a Resource group

* In my project, create a `main.tf` for the configutation
	* run `terraform init`
* Create a resource group via Terraform
	* run `terraform apply`
* Add new items to `.gitignore`


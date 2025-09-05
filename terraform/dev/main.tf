terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 3.0.2"
    }
  }
  required_version = ">= 1.1.0"
}

provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg_dev" {
  name     = "atlas-dev-rg"
  location = "westus2"
}

resource "azurerm_application_insights" "app_insights_dev" {
  name                = "atlas-dev-app-insights"
  location            = azurerm_resource_group.rg_dev.location
  resource_group_name = azurerm_resource_group.rg_dev.name
  application_type    = "web"  
}
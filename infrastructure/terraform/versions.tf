terraform {
  required_version = ">= 1.7.0"
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    azapi = {
      source  = "Azure/azapi"
      version = "~> 2.0"
    }
  }
  backend "azurerm" {}
}

# Rehearsal cleanup temporarily adds a provider override requiring an empty
# resource group before deleting it; the normal provider is not rewritten.
provider "azurerm" {
  features {}
  subscription_id = var.subscription_id
}

# Discovery storage is provisioned through ARM. Shared key data access is disabled.
provider "azurerm" {
  alias           = "discovery_cache"
  subscription_id = var.subscription_id

  features {
    storage {
      data_plane_available = false
    }
  }
}

terraform {
  required_providers {
    azurerm = {
      source = "hashicorp/azurerm"
    }
  }
}

variable "name" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "principal_id" { type = string }
variable "tags" { type = map(string) }

resource "azurerm_storage_account" "atlas" {
  name                            = var.name
  resource_group_name             = var.resource_group_name
  location                        = var.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  account_kind                    = "StorageV2"
  access_tier                     = "Hot"
  min_tls_version                 = "TLS1_2"
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = false
  tags                            = var.tags
}

resource "azurerm_storage_container" "discovery" {
  name                  = "discovery"
  storage_account_id    = azurerm_storage_account.atlas.id
  container_access_type = "private"
}

resource "azurerm_role_assignment" "runtime" {
  scope                = azurerm_storage_container.discovery.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_storage_management_policy" "expire" {
  storage_account_id = azurerm_storage_account.atlas.id

  rule {
    name    = "purge-expired-discovery-cache"
    enabled = true
    filters {
      prefix_match = ["discovery/"]
      blob_types   = ["blockBlob"]
    }
    actions {
      base_blob {
        delete_after_days_since_modification_greater_than = 1
      }
    }
  }
}

output "container_uri" {
  value      = "${azurerm_storage_account.atlas.primary_blob_endpoint}${azurerm_storage_container.discovery.name}"
  depends_on = [azurerm_role_assignment.runtime, azurerm_storage_management_policy.expire]
}

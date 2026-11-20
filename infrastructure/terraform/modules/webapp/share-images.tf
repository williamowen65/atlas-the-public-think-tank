resource "azurerm_storage_account" "share_images" {
  name                            = "atlas${substr(sha256(lower(var.identity_id)), 0, 19)}"
  resource_group_name             = var.resource_group_name
  location                        = var.location
  account_tier                    = "Standard"
  account_replication_type        = "LRS"
  min_tls_version                 = "TLS1_2"
  allow_nested_items_to_be_public = false
  shared_access_key_enabled       = false
  tags                            = var.tags
}

resource "azurerm_storage_container" "share_images" {
  name                  = "share-images"
  storage_account_id    = azurerm_storage_account.share_images.id
  container_access_type = "private"
}

resource "azurerm_role_assignment" "share_images" {
  scope                = "${azurerm_storage_account.share_images.id}/blobServices/default/containers/${azurerm_storage_container.share_images.name}"
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = var.identity_principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_storage_management_policy" "share_images" {
  storage_account_id = azurerm_storage_account.share_images.id

  rule {
    name    = "expire-share-image-cache"
    enabled = true

    filters {
      prefix_match = ["share-images/"]
      blob_types   = ["blockBlob"]
    }

    actions {
      base_blob {
        delete_after_days_since_modification_greater_than = 7
      }
    }
  }
}

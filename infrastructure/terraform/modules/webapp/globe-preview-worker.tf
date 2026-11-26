variable "globe_preview_worker_image" {
  type     = string
  default  = null
  nullable = true
}

variable "log_analytics_workspace_id" {
  type = string
}

locals {
  # Container Apps names are limited in length. Include a hash of the site name.
  globe_worker_name  = "atlas-globe-${substr(sha256(var.name), 0, 12)}"
  globe_worker_count = var.globe_preview_worker_image == null ? 0 : 1
}

resource "azurerm_container_app_environment" "globe_preview" {
  count                      = local.globe_worker_count
  name                       = local.globe_worker_name
  location                   = var.location
  resource_group_name        = var.resource_group_name
  log_analytics_workspace_id = var.log_analytics_workspace_id
  tags                       = var.tags
}

resource "azurerm_user_assigned_identity" "globe_preview_worker" {
  count               = local.globe_worker_count
  name                = "${var.name}-globe-worker"
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

resource "azurerm_role_assignment" "globe_preview_worker" {
  count                = local.globe_worker_count
  scope                = azurerm_role_assignment.share_images.scope
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.globe_preview_worker[0].principal_id
  principal_type       = "ServicePrincipal"
}

resource "azurerm_container_app_job" "globe_preview" {
  count                        = local.globe_worker_count
  name                         = local.globe_worker_name
  location                     = var.location
  resource_group_name          = var.resource_group_name
  container_app_environment_id = azurerm_container_app_environment.globe_preview[0].id
  replica_timeout_in_seconds   = 1800
  replica_retry_limit          = 0
  tags                         = var.tags

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.globe_preview_worker[0].id]
  }

  manual_trigger_config {
    parallelism              = 1
    replica_completion_count = 1
  }

  template {
    container {
      name   = "globe-preview"
      image  = var.globe_preview_worker_image
      cpu    = 2
      memory = "4Gi"

      env {
        name  = "Atlas__Seo__PublicOrigin"
        value = coalesce(var.seo_public_origin, "https://${var.name}.azurewebsites.net")
      }
      env {
        name  = "Atlas__Seo__BlobContainerUrl"
        value = "${azurerm_storage_account.share_images.primary_blob_endpoint}${azurerm_storage_container.share_images.name}"
      }
      env {
        name  = "Atlas__Seo__BlobIdentityClientId"
        value = azurerm_user_assigned_identity.globe_preview_worker[0].client_id
      }
    }
  }
}

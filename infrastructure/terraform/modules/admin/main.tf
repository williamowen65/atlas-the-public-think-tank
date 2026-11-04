variable "name" { type = string }
variable "variant" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "service_plan_id" { type = string }
variable "app_subnet_id" { type = string }
variable "identity_id" { type = string }
variable "sql_connection" { type = string }
variable "runtime_identity_client_id" { type = string }
variable "admin_identity_client_id" { type = string }
variable "tags" { type = map(string) }
variable "enable_bootstrap" { type = bool }
variable "bootstrap_username" { type = string }
variable "bootstrap_password" {
  type      = string
  sensitive = true
}

# Dedicated administration host on the SAME App Service plan as Atlas.
# No public HTTP routes are admitted. Azure SCM remains Entra-protected so
# authorized GitHub OIDC deployment can upload and run triggered WebJobs.
# The WebJob runtime uses VNet integration to access private SQL.
resource "azurerm_linux_web_app" "admin" {
  name                                      = var.name
  location                                  = var.location
  resource_group_name                       = var.resource_group_name
  service_plan_id                           = var.service_plan_id
  virtual_network_subnet_id                 = var.app_subnet_id
  https_only                                = true
  ftp_publish_basic_authentication_enabled = false
  webdeploy_publish_basic_authentication_enabled = false
  tags                                      = var.tags

  identity {
    type         = "UserAssigned"
    identity_ids = [var.identity_id]
  }

  site_config {
    always_on                         = true
    ftps_state                        = "Disabled"
    minimum_tls_version               = "1.2"
    ip_restriction_default_action     = "Deny"
    scm_ip_restriction_default_action = "Allow"

    application_stack {
      dotnet_version = "10.0"
    }
  }

  app_settings = merge({
    ASPNETCORE_ENVIRONMENT        = "Production"
    WEBSITE_SKIP_RUNNING_KUDUAGENT = "false"
    ATLAS_DEPLOYMENT_VARIANT      = var.variant
    ATLAS_SQL_CONNECTION_STRING   = var.sql_connection
    ATLAS_RUNTIME_MI_CLIENT_ID    = var.runtime_identity_client_id
    ATLAS_ADMIN_MI_CLIENT_ID      = var.admin_identity_client_id
    }, var.enable_bootstrap ? {
    # Only present during the manually approved first-time identity bootstrap.
    # Toggle back to false and apply Terraform after successful grant.
    ATLAS_BOOTSTRAP_SQL_USERNAME = var.bootstrap_username
    ATLAS_BOOTSTRAP_SQL_PASSWORD = var.bootstrap_password
  } : {})
}

output "name" {
  value = azurerm_linux_web_app.admin.name
}

output "identity_principal_id" {
  value = azurerm_linux_web_app.admin.identity[0].principal_id
}

variable "name" { type = string }
variable "variant" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "service_plan_id" { type = string }
variable "app_subnet_id" { type = string }
variable "app_service_sku" { type = string }
variable "insights_connection" { type = string }
variable "identity_id" { type = string }
variable "connection_string" {
  type      = string
  default   = null
  sensitive = true
}
variable "tags" { type = map(string) }

resource "azurerm_linux_web_app" "atlas" {
  name                                     = var.name
  resource_group_name                      = var.resource_group_name
  location                                 = var.location
  service_plan_id                          = var.service_plan_id
  virtual_network_subnet_id                = var.app_subnet_id
  https_only                               = true
  ftp_publish_basic_authentication_enabled = false
  webdeploy_publish_basic_authentication_enabled = false
  tags                                     = var.tags

  identity {
    type         = "UserAssigned"
    identity_ids = [var.identity_id]
  }

  site_config {
    always_on           = !contains(["F1", "D1"], var.app_service_sku)
    ftps_state          = "Disabled"
    minimum_tls_version = "1.2"

    application_stack {
      dotnet_version = "10.0"
    }
  }

  # Passwordless connection: the corresponding database identity is provisioned
  # separately by the protected initial bootstrap WebJob before app startup.
  app_settings = merge({
    ASPNETCORE_ENVIRONMENT                      = "Production"
    APPLICATIONINSIGHTS_CONNECTION_STRING       = var.insights_connection
    ATLAS_DEPLOYMENT_VARIANT                    = var.variant
    }, var.connection_string == null ? {} : {
    ATLAS_SQL_CONNECTION_STRING = var.connection_string
  })
}

output "name" { value = azurerm_linux_web_app.atlas.name }
output "url" { value = format("https://%s", azurerm_linux_web_app.atlas.default_hostname) }
output "possible_outbound_ipv4" {
  value = azurerm_linux_web_app.atlas.possible_outbound_ip_address_list
}

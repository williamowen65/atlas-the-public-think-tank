variable "name" { type = string }
variable "variant" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "service_plan_id" { type = string }
variable "app_service_sku" { type = string }
variable "insights_connection" { type = string }
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
  https_only                               = true
  ftp_publish_basic_authentication_enabled = false
  tags                                     = var.tags

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on           = !contains(["F1", "D1"], var.app_service_sku)
    ftps_state          = "Disabled"
    minimum_tls_version = "1.2"

    application_stack {
      dotnet_version = "10.0"
    }
  }

  # The separate app-specific SQL login is required before this host can start.
  app_settings = merge({
    ASPNETCORE_ENVIRONMENT                      = "Production"
    APPLICATIONINSIGHTS_CONNECTION_STRING       = var.insights_connection
    ApplicationInsightsAgent_EXTENSION_VERSION  = "~3"
    XDT_MicrosoftApplicationInsights_Mode       = "recommended"
    XDT_MicrosoftApplicationInsights_PreemptSdk = "1"
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

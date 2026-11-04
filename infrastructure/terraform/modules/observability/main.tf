variable "name_prefix" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "tags" { type = map(string) }

resource "azurerm_log_analytics_workspace" "atlas" {
  name                = format("%s-logs", var.name_prefix)
  location            = var.location
  resource_group_name = var.resource_group_name
  sku                 = "PerGB2018"
  retention_in_days   = 30
  tags                = var.tags
}

resource "azurerm_application_insights" "atlas" {
  name                = format("%s-insights", var.name_prefix)
  location            = var.location
  resource_group_name = var.resource_group_name
  workspace_id        = azurerm_log_analytics_workspace.atlas.id
  application_type    = "web"
  tags                = var.tags
}

output "insights_name" {
  value = azurerm_application_insights.atlas.name
}
output "insights_connection" {
  value = azurerm_application_insights.atlas.connection_string
}

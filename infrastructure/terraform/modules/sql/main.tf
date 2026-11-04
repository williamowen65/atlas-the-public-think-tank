variable "name_prefix" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "admin_username" { type = string }
variable "admin_password" {
  type      = string
  sensitive = true
}
variable "variants" { type = set(string) }
variable "allowed_ipv4" { type = map(string) }
variable "tags" { type = map(string) }

resource "azurerm_mssql_server" "atlas" {
  name                          = format("%s-sql", var.name_prefix)
  resource_group_name           = var.resource_group_name
  location                      = var.location
  version                       = "12.0"
  administrator_login           = var.admin_username
  administrator_login_password  = var.admin_password
  minimum_tls_version           = "1.2"
  public_network_access_enabled = true
  tags                          = var.tags
}

# AzureRM does not currently expose the useFreeLimit database creation flag.
# AzAPI manages the same ARM child resource with explicit free-offer settings.
# The Azure SQL free tier automatically pauses after inactivity and when the
# monthly allowance is exhausted. Never silently substitute a billable Basic DB.
resource "azapi_resource" "variant" {
  for_each  = var.variants
  type      = "Microsoft.Sql/servers/databases@2025-01-01"
  name      = format("%s-%s", var.name_prefix, each.key)
  parent_id = azurerm_mssql_server.atlas.id
  location  = var.location
  tags      = merge(var.tags, { variant = each.key })

  body = {
    sku = {
      name     = "GP_S_Gen5_2"
      tier     = "GeneralPurpose"
      family   = "Gen5"
      capacity = 2
    }
    properties = {
      useFreeLimit                 = true
      freeLimitExhaustionBehavior  = "AutoPause"
      maxSizeBytes                 = 34359738368
      requestedBackupStorageRedundancy = "Local"
    }
  }

  lifecycle {
    prevent_destroy = true
  }
}

resource "azurerm_mssql_firewall_rule" "approved" {
  for_each         = var.allowed_ipv4
  name             = format("Atlas-%s", each.key)
  server_id        = azurerm_mssql_server.atlas.id
  start_ip_address = each.value
  end_ip_address   = each.value
}

output "server_fqdn" {
  value = azurerm_mssql_server.atlas.fully_qualified_domain_name
}
output "database_names" {
  value = { for variant, db in azapi_resource.variant : variant => format("%s-%s", var.name_prefix, variant) }
}

variable "name_prefix" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "admin_username" { type = string }
variable "admin_password" {
  type      = string
  sensitive = true
}
variable "database_sku" { type = string }
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

resource "azurerm_mssql_database" "variant" {
  for_each  = var.variants
  name      = format("%s-%s", var.name_prefix, each.key)
  server_id = azurerm_mssql_server.atlas.id
  sku_name  = var.database_sku
  tags      = merge(var.tags, { variant = each.key })

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
  value = { for variant, db in azurerm_mssql_database.variant : variant => db.name }
}

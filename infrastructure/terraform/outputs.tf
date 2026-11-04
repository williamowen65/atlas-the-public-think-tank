output "resource_group" {
  value = azurerm_resource_group.atlas.name
}
output "site_names" {
  value = { for variant, app in module.sites : variant => app.name }
}
output "site_urls" {
  value = { for variant, app in module.sites : variant => app.url }
}
output "sql_server_fqdn" {
  value = module.sql.server_fqdn
}
output "database_names" {
  value = module.sql.database_names
}
output "application_insights_name" {
  value = module.observability.insights_name
}

output "virtual_network" {
  value = module.network.vnet_name
}

output "sql_private_endpoint_id" {
  value = module.network.sql_private_endpoint_id
}

output "sql_private_dns_zone" {
  value = module.network.sql_private_dns_zone
}

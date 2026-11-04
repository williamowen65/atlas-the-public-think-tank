output "resource_group" {
  value = azurerm_resource_group.atlas.name
}
output "site_names" {
  value = { for variant, app in module.sites : variant => app.name }
}
output "site_urls" {
  value = { for variant, app in module.sites : variant => app.url }
}
output "possible_outbound_ipv4" {
  description = "Add each required outbound IP as a named sql_allowed_ipv4 entry, then reapply."
  value       = { for variant, app in module.sites : variant => app.possible_outbound_ipv4 }
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

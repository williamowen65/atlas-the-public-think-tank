locals {
  variants = var.enable_demo ? toset(["live", "demo"]) : toset(["live"])
  tags     = merge(var.tags, { managed_by = "terraform" })
}

resource "azurerm_resource_group" "atlas" {
  name     = format("%s-rg", var.name_prefix)
  location = var.location
  tags     = local.tags
}

module "observability" {
  source              = "./modules/observability"
  name_prefix         = var.name_prefix
  location            = azurerm_resource_group.atlas.location
  resource_group_name = azurerm_resource_group.atlas.name
  tags                = local.tags
}

module "sql" {
  source              = "./modules/sql"
  name_prefix         = var.name_prefix
  location            = azurerm_resource_group.atlas.location
  resource_group_name = azurerm_resource_group.atlas.name
  admin_username      = var.sql_admin_username
  admin_password      = var.sql_admin_password
  variants            = local.variants
  allowed_ipv4        = var.sql_allowed_ipv4
  tags                = local.tags
}

resource "azurerm_service_plan" "atlas" {
  name                = format("%s-plan", var.name_prefix)
  resource_group_name = azurerm_resource_group.atlas.name
  location            = azurerm_resource_group.atlas.location
  os_type             = "Linux"
  sku_name            = var.app_service_sku
  tags                = local.tags
}

module "sites" {
  for_each = local.variants
  source   = "./modules/webapp"

  name                = format("%s-%s", var.name_prefix, each.key)
  variant             = each.key
  location            = azurerm_resource_group.atlas.location
  resource_group_name = azurerm_resource_group.atlas.name
  service_plan_id     = azurerm_service_plan.atlas.id
  app_service_sku     = var.app_service_sku
  insights_connection = module.observability.insights_connection
  connection_string   = lookup(var.runtime_connection_strings, each.key, null)
  tags                = merge(local.tags, { variant = each.key })
}

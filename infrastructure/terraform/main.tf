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
  tags                = local.tags
}

module "network" {
  source              = "./modules/network"
  name_prefix         = var.name_prefix
  location            = azurerm_resource_group.atlas.location
  resource_group_name = azurerm_resource_group.atlas.name
  sql_server_id       = module.sql.server_id
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


# Each public web app and its management WebJob have DISTINCT Azure identities.
# Each database will later contain only the matching pair of contained users.
resource "azurerm_user_assigned_identity" "runtime" {
  for_each            = local.variants
  name                = format("%s-mi-%s-web", var.name_prefix, each.key)
  location            = azurerm_resource_group.atlas.location
  resource_group_name = azurerm_resource_group.atlas.name
  tags                = merge(local.tags, { variant = each.key, role = "runtime" })
}

resource "azurerm_user_assigned_identity" "operations" {
  for_each            = local.variants
  name                = format("%s-mi-%s-ops", var.name_prefix, each.key)
  location            = azurerm_resource_group.atlas.location
  resource_group_name = azurerm_resource_group.atlas.name
  tags                = merge(local.tags, { variant = each.key, role = "database-operations" })
}

module "sites" {
  for_each = local.variants
  source   = "./modules/webapp"

  name                = format("%s-%s", var.name_prefix, each.key)
  variant             = each.key
  location            = azurerm_resource_group.atlas.location
  resource_group_name = azurerm_resource_group.atlas.name
  service_plan_id     = azurerm_service_plan.atlas.id
  app_subnet_id       = module.network.app_subnet_id
  app_service_sku     = var.app_service_sku
  insights_connection = module.observability.insights_connection
  identity_id         = azurerm_user_assigned_identity.runtime[each.key].id
  connection_string   = format(
    "Server=tcp:%s,1433;Database=%s;Authentication=Active Directory Managed Identity;User Id=%s;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30",
    module.sql.server_fqdn,
    module.sql.database_names[each.key],
    azurerm_user_assigned_identity.runtime[each.key].client_id
  )
  tags                = merge(local.tags, { variant = each.key })
}

# Management-only WebJob hosts share the existing B1 plan but do not share SQL
# identities or HTTP ingress with the public live/demo applications.
module "admin_sites" {
  for_each = local.variants
  source   = "./modules/admin"

  name                       = format("%s-%s-ops", var.name_prefix, each.key)
  variant                    = each.key
  location                   = azurerm_resource_group.atlas.location
  resource_group_name        = azurerm_resource_group.atlas.name
  service_plan_id            = azurerm_service_plan.atlas.id
  app_subnet_id              = module.network.app_subnet_id
  identity_id                = azurerm_user_assigned_identity.operations[each.key].id
  runtime_identity_client_id = azurerm_user_assigned_identity.runtime[each.key].client_id
  admin_identity_client_id   = azurerm_user_assigned_identity.operations[each.key].client_id
  sql_connection = format(
    "Server=tcp:%s,1433;Database=%s;Authentication=Active Directory Managed Identity;User Id=%s;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30",
    module.sql.server_fqdn,
    module.sql.database_names[each.key],
    azurerm_user_assigned_identity.operations[each.key].client_id
  )
  enable_bootstrap   = var.enable_sql_bootstrap
  bootstrap_username = var.sql_admin_username
  bootstrap_password = var.sql_admin_password
  tags               = merge(local.tags, { variant = each.key, role = "database-operations" })
}

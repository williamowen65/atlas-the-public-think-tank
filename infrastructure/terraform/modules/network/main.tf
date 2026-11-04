variable "name_prefix" { type = string }
variable "location" { type = string }
variable "resource_group_name" { type = string }
variable "sql_server_id" { type = string }
variable "tags" { type = map(string) }

# The VNet and both subnets are free resources. App Service VNet integration
# requires a paid Basic-or-higher plan, and the SQL Private Endpoint and DNS
# zone have their own charges. These addresses are internal only.
resource "azurerm_virtual_network" "atlas" {
  name                = format("%s-vnet", var.name_prefix)
  location            = var.location
  resource_group_name = var.resource_group_name
  address_space       = ["10.42.0.0/16"]
  tags                = var.tags
}

# App Service outbound integration must use a dedicated delegated subnet.
# Two apps sharing a plan may integrate into the same subnet.
resource "azurerm_subnet" "app" {
  name                 = "snet-app"
  resource_group_name  = var.resource_group_name
  virtual_network_name = azurerm_virtual_network.atlas.name
  address_prefixes     = ["10.42.1.0/26"]

  delegation {
    name = "app-service"
    service_delegation {
      name    = "Microsoft.Web/serverFarms"
      actions = ["Microsoft.Network/virtualNetworks/subnets/action"]
    }
  }
}

# SQL is only reachable from the VNet private endpoint, never via public IP.
resource "azurerm_subnet" "private_endpoints" {
  name                              = "snet-private-endpoints"
  resource_group_name               = var.resource_group_name
  virtual_network_name              = azurerm_virtual_network.atlas.name
  address_prefixes                  = ["10.42.2.0/27"]
  private_endpoint_network_policies = "NetworkSecurityGroupEnabled"
}

# NSG acts as a stateful subnet firewall for the SQL private endpoint.
# Permit SQL/TCP 1433 only from the App Service integration subnet. The
# deny rule overrides Azure's default AllowVNetInBound for other VNet sources.
resource "azurerm_network_security_group" "sql" {
  name                = format("%s-nsg-sql", var.name_prefix)
  location            = var.location
  resource_group_name = var.resource_group_name
  tags                = var.tags

  security_rule {
    name                       = "AllowAppToSql"
    priority                   = 100
    direction                  = "Inbound"
    access                     = "Allow"
    protocol                   = "Tcp"
    source_port_range          = "*"
    destination_port_range     = "1433"
    source_address_prefix      = "10.42.1.0/26"
    destination_address_prefix = "10.42.2.0/27"
  }

  security_rule {
    name                       = "DenyOtherVNetToSql"
    priority                   = 200
    direction                  = "Inbound"
    access                     = "Deny"
    protocol                   = "*"
    source_port_range          = "*"
    destination_port_range     = "*"
    source_address_prefix      = "VirtualNetwork"
    destination_address_prefix = "10.42.2.0/27"
  }
}

resource "azurerm_subnet_network_security_group_association" "sql" {
  subnet_id                 = azurerm_subnet.private_endpoints.id
  network_security_group_id = azurerm_network_security_group.sql.id
}

# DNS makes the normal Azure SQL FQDN resolve to its private IP from the VNet.
# Connection strings must keep <server>.database.windows.net, not the private IP.
resource "azurerm_private_dns_zone" "sql" {
  name                = "privatelink.database.windows.net"
  resource_group_name = var.resource_group_name
  tags                = var.tags
}

resource "azurerm_private_dns_zone_virtual_network_link" "sql" {
  name                  = format("%s-sql-link", var.name_prefix)
  resource_group_name   = var.resource_group_name
  private_dns_zone_name = azurerm_private_dns_zone.sql.name
  virtual_network_id    = azurerm_virtual_network.atlas.id
  registration_enabled  = false
  tags                  = var.tags
}

resource "azurerm_private_endpoint" "sql" {
  name                = format("%s-pe-sql", var.name_prefix)
  location            = var.location
  resource_group_name = var.resource_group_name
  subnet_id           = azurerm_subnet.private_endpoints.id
  tags                = var.tags

  private_service_connection {
    name                           = format("%s-sql-connection", var.name_prefix)
    private_connection_resource_id = var.sql_server_id
    subresource_names              = ["sqlServer"]
    is_manual_connection           = false
  }

  private_dns_zone_group {
    name                 = "sql"
    private_dns_zone_ids = [azurerm_private_dns_zone.sql.id]
  }

  depends_on = [
    azurerm_private_dns_zone_virtual_network_link.sql,
    azurerm_subnet_network_security_group_association.sql
  ]
}

output "app_subnet_id" {
  value = azurerm_subnet.app.id
}

output "sql_private_endpoint_id" {
  value = azurerm_private_endpoint.sql.id
}

output "sql_private_dns_zone" {
  value = azurerm_private_dns_zone.sql.name
}

output "vnet_name" {
  value = azurerm_virtual_network.atlas.name
}

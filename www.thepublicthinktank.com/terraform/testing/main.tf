terraform {
  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = ">= 4.0.0"
    }
  }
  required_version = ">= 1.1.0"
}

provider "azurerm" {
  features {}
  subscription_id = "cbd49d37-218d-4383-9549-d36bbeea48af"
}

resource "random_pet" "server_name" {
  length    = 2
  separator = "-"
}

resource "azurerm_resource_group" "rg_testing" {
  name     = "atlas-testing-rg"
  location = "westus2"
}


resource "azurerm_log_analytics_workspace" "law_testing" {
  name                = "atlas-testing-law"
  location            = azurerm_resource_group.rg_testing.location
  resource_group_name = azurerm_resource_group.rg_testing.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_application_insights" "app_insights_testing" {
  name                = "atlas-testing-app-insights"
  location            = azurerm_resource_group.rg_testing.location
  resource_group_name = azurerm_resource_group.rg_testing.name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.law_testing.id
}


resource "azurerm_mssql_server" "mssql_server_testing" {
  name                         = "atlas-testing-${random_pet.server_name.id}-sqlserver"
  resource_group_name          = azurerm_resource_group.rg_testing.name
  location                     = azurerm_resource_group.rg_testing.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password
}

resource "azurerm_mssql_database" "mssql_database_testing" {
  name         = "atlas-testing-${random_pet.server_name.id}-db"
  server_id    = azurerm_mssql_server.mssql_server_testing.id
  collation    = "SQL_Latin1_General_CP1_CI_AS"
  license_type = "LicenseIncluded"
  max_size_gb  = 2
  sku_name     = "Basic"
  enclave_type = "VBS"
}

resource "azurerm_mssql_firewall_rule" "mssql_firewall_rule_testing" {
  name             = "AllowMyIP"
  server_id        = azurerm_mssql_server.mssql_server_testing.id
  start_ip_address = "208.53.112.211"
  end_ip_address   = "208.53.112.211"
}

# TWO STEP APPLY PROCESS
# First: terraform apply -target="azurerm_linux_web_app.web-app-testing"
# Second: terraform apply  
# the second will over the firewall rule
resource "azurerm_mssql_firewall_rule" "app_service_fw" {
  for_each = toset(split(",", azurerm_linux_web_app.web-app-testing.possible_outbound_ip_addresses))

  name             = "AllowAppService-${each.key}"
  server_id        = azurerm_mssql_server.mssql_server_testing.id
  start_ip_address = each.value
  end_ip_address   = each.value
}

resource "azurerm_service_plan" "service_plan_testing" {
  name                = "atlas-testing-service-plan"
  resource_group_name = azurerm_resource_group.rg_testing.name
  location            = azurerm_resource_group.rg_testing.location
  os_type             = "Linux"
  sku_name            = "F1" # Free tier
}


resource "azurerm_linux_web_app" "web-app-testing" {
  name                = "atlas-testing-${random_pet.server_name.id}-web-app"
  resource_group_name = azurerm_resource_group.rg_testing.name
  location            = azurerm_service_plan.service_plan_testing.location
  service_plan_id     = azurerm_service_plan.service_plan_testing.id

  site_config {
    always_on = false
  }

  app_settings = {
    "ASPNETCORE_ENVIRONMENT"          = var.aspnetcore_env
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = azurerm_application_insights.app_insights_testing.connection_string
    "SMTP_HOST" = var.smtp_host
    "SMTP_USERNAME" = var.smtp_username
    "SMTP_PASSWORD" = var.smtp_password
  }

  connection_string {
    name = "DefaultConnection"
    type = "SQLAzure"
    value = format(
      "Server=tcp:%s.database.windows.net,1433;Database=%s;User ID=%s;Password=%s;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;",
      azurerm_mssql_server.mssql_server_testing.name,
      azurerm_mssql_database.mssql_database_testing.name,
      azurerm_mssql_server.mssql_server_testing.administrator_login,
      azurerm_mssql_server.mssql_server_testing.administrator_login_password
    )
  }
}


output "web_app_connection_string" {
  value = [
    for cs in azurerm_linux_web_app.web-app-testing.connection_string : cs.value
  ][0]
  description = "The connection string for the Linux web app."
  sensitive   = true
}



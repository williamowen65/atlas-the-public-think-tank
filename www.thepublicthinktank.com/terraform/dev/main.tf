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

resource "azurerm_resource_group" "rg_dev" {
  name     = "atlas-dev-rg"
  location = "westus2"
}


resource "azurerm_log_analytics_workspace" "law_dev" {
  name                = "atlas-dev-law"
  location            = azurerm_resource_group.rg_dev.location
  resource_group_name = azurerm_resource_group.rg_dev.name
  sku                 = "PerGB2018"
  retention_in_days   = 30
}

resource "azurerm_application_insights" "app_insights_dev" {
  name                = "atlas-dev-app-insights"
  location            = azurerm_resource_group.rg_dev.location
  resource_group_name = azurerm_resource_group.rg_dev.name
  application_type    = "web"
  workspace_id        = azurerm_log_analytics_workspace.law_dev.id
}


resource "azurerm_mssql_server" "mssql_server_dev" {
  name                         = "atlas-dev-${random_pet.server_name.id}-sqlserver"
  resource_group_name          = azurerm_resource_group.rg_dev.name
  location                     = azurerm_resource_group.rg_dev.location
  version                      = "12.0"
  administrator_login          = var.sql_admin_username
  administrator_login_password = var.sql_admin_password
}

resource "azurerm_mssql_database" "mssql_database_dev" {
  name         = "atlas-dev-${random_pet.server_name.id}-db"
  server_id    = azurerm_mssql_server.mssql_server_dev.id
  collation    = "SQL_Latin1_General_CP1_CI_AS"
  license_type = "LicenseIncluded"
  max_size_gb  = 2
  sku_name     = "Basic" 
  enclave_type = "VBS"
}

resource "azurerm_service_plan" "service_plan_dev" {
  name                = "atlas-dev-service-plan"
  resource_group_name = azurerm_resource_group.rg_dev.name
  location            = azurerm_resource_group.rg_dev.location
  os_type             = "Linux"
  sku_name            = "F1" # Free tier
}

resource "azurerm_linux_web_app" "web-app-dev" {
  name                = "atlas-dev-${random_pet.server_name.id}-web-app"
  resource_group_name = azurerm_resource_group.rg_dev.name
  location            = azurerm_service_plan.service_plan_dev.location
  service_plan_id     = azurerm_service_plan.service_plan_dev.id

  site_config {
    always_on = false  
  }
}
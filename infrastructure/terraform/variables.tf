variable "subscription_id" { type = string }
variable "location" {
  type    = string
  default = "westus2"
}
variable "name_prefix" {
  description = "Globally unique prefix for Azure resource names."
  type        = string
  validation {
    condition     = can(regex("^[a-z][a-z0-9-]{4,25}[a-z0-9]$", var.name_prefix))
    error_message = "Use 6-27 lowercase letters, digits or hyphens."
  }
}
variable "enable_demo" {
  description = "Create separate demo app and free-offer SQL database. Disable explicitly to provision only live."
  type        = bool
  default     = true
}
variable "app_service_sku" {
  type    = string
  default = "B1"
  validation {
    condition     = !contains(["F1", "D1"], upper(var.app_service_sku))
    error_message = "SQL Private Link requires App Service VNet integration; Free/Shared App Service plans do not support it."
  }
}
variable "sql_admin_username" {
  type    = string
  default = "atlasprovisioner"
}
variable "sql_admin_password" {
  description = "Provisioning-only SQL admin password. Protected Terraform state still contains this."
  type        = string
  sensitive   = true
}
variable "runtime_connection_strings" {
  description = "Least-privilege SQL connections keyed by live/demo. Never use SQL admin for app access."
  type        = map(string)
  default     = {}
  sensitive   = true
}
variable "tags" {
  type = map(string)
  default = {
    project = "atlas"
    purpose = "school-capstone"
  }
}

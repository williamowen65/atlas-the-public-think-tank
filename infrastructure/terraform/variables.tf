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
  description = "Create separate demo app and SQL database. This incurs additional DB cost."
  type        = bool
  default     = false
}
variable "app_service_sku" {
  type    = string
  default = "F1"
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
variable "sql_allowed_ipv4" {
  description = "Exact allowed SQL client IPs, keyed by short names. Empty means no firewall access."
  type        = map(string)
  default     = {}
  validation {
    condition = alltrue([
      for ip in values(var.sql_allowed_ipv4) :
      can(cidrhost(format("%s/32", ip), 0)) && ip != "0.0.0.0" && !strcontains(ip, ":")
    ])
    error_message = "Use individual IPv4 addresses. Never allow all of Azure (0.0.0.0)."
  }
}
variable "tags" {
  type = map(string)
  default = {
    project = "atlas"
    purpose = "school-capstone"
  }
}

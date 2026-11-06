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
variable "deployment_variants" {
  description = "Explicit live/demo application set. Null preserves the enable_demo compatibility setting."
  type        = set(string)
  default     = null
  validation {
    condition = var.deployment_variants == null ? true : (
      length(var.deployment_variants) > 0 &&
      alltrue([for variant in var.deployment_variants : contains(["live", "demo"], variant)])
    )
    error_message = "Select live, demo, or both; an empty or unknown variant set is invalid."
  }
}

variable "app_service_sku" {
  type    = string
  default = "B1"
  validation {
    condition     = !contains(["F1", "D1"], upper(var.app_service_sku))
    error_message = "SQL Private Link requires App Service VNet integration; Free/Shared App Service plans do not support it."
  }
}
variable "sql_entra_admin_login" {
  description = "Display name of the Microsoft Entra user/group permitted to administer Azure SQL. Prefer an operator security group."
  type        = string
}
variable "sql_entra_admin_object_id" {
  description = "Microsoft Entra object ID of the designated SQL administrator."
  type        = string
  validation {
    condition     = can(regex("^[0-9a-fA-F-]{36}$", var.sql_entra_admin_object_id))
    error_message = "Provide the 36-character Entra principal object GUID."
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
variable "demo_seed_password" {
  description = "Unique high-entropy password for seeded demo-only Identity accounts; never print or commit."
  type        = string
  default     = null
  sensitive   = true
}

variable "enable_sql_bootstrap" {
  description = "Temporary: expose SQL admin credentials only to private administrative WebJobs for the initial contained-user bootstrap. Turn back off and apply immediately afterward."
  type        = bool
  default     = false
}

variable "tags" {
  type = map(string)
  default = {
    project = "atlas"
    purpose = "school-capstone"
  }
}

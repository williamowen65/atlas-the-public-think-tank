variable "sql_admin_username" {
  description = "Admin username for SQL Server"
  type        = string
  sensitive   = true
}

variable "sql_admin_password" {
  description = "Admin password for SQL Server"
  type        = string
  sensitive   = true
}

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

variable "aspnetcore_env" {
  description = "Development | Staging | Production"
  type        = string
  sensitive   = false
}

variable "smtp_host" {
  description = "The SMTP provider: A url that is the entry point for smtp login"
  type        = string
  sensitive   = true
}

variable "smtp_username" {
  description = "The SMTP Username"
  type        = string
  sensitive   = true
}

variable "smtp_password" {
  description = "Environment variable, a password for stmp server access"
  type        = string
  sensitive   = true
}
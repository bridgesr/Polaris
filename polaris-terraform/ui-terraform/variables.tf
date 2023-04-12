#################### Variables ####################

variable "resource_name_prefix" {
  type    = string
  default = "polaris"
}

variable "networking_resource_name_suffix" {
  default = "networking"
}

variable "env" {
  type = string
}

variable "location" {
  description = "The location of this resource"
  type        = string
}

variable "app_service_plan_web_sku" {
  type = string
}

variable "app_service_plan_gateway_sku" {
  type = string
}

variable "environment_tag" {
  type        = string
  description = "Environment tag value"
}

variable "polaris_webapp_details" {
  type = object({
    valid_audience = string
    valid_scopes   = string
    valid_roles    = string
  })
}

variable "dns_server" {
  type = string
}

variable "polaris_ui_sub_folder" {
  type = string
  // this value must match the PUBLIC_URL=... value
  //  as seen in the ui project top-level package.json
  //  scripts section.
  default = "polaris-ui"
}

variable "terraform_service_principal_display_name" {
  type = string
}
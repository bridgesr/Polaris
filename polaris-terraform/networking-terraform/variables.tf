
#################### Variables ####################

variable "resource_name_prefix" {
  default = "networking"
}

variable "environment" {
  type = object({
    name  = string
    alias = string
  })
}

variable "location" {
  default = "UK South"
}

variable "vnetAddressSpace" {
}

variable "cmsServicesSubnet" {
}

variable "ddeiServicesSubnet" {
}

variable "polarisServicesSubnet" {
}

variable "digital-platform-shared-subscription-id" {
  default = "8eeb7cbd-fa86-46be-9112-c72428713fc8"
}
data "azurerm_linux_function_app" "fa_coordinator" {
  name                = "fa-${local.pipeline_resource_name}-coordinator"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azurerm_windows_function_app" "fa_pdf_generator" {
  name                = "fa-${local.pipeline_resource_name}-pdf-generator"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azurerm_linux_function_app" "fa_text_extractor" {
  name                = "fa-${local.pipeline_resource_name}-text-extractor"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azurerm_storage_container" "pipeline_storage_container" {
  name                 = "documents"
  storage_account_name = "sacps${var.env != "prod" ? var.env : ""}polarispipeline"
}

data "azurerm_storage_account" "pipeline_storage_account" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polarispipeline"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}

data "azuread_service_principal" "computer_vision_sp" {
  display_name        = "cv-${local.pipeline_resource_name}"
}

data "azurerm_search_service" "pipeline_search_service" {
  name                = "ss-${local.pipeline_resource_name}"
  resource_group_name = "rg-${local.pipeline_resource_name}"
}
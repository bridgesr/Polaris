#################### App Service Plan ####################

resource "azurerm_service_plan" "asp_polaris_gateway" {
  #checkov:skip=CKV_AZURE_212:Ensure App Service has a minimum number of instances for fail over
  #checkov:skip=CKV_AZURE_225:Ensure the App Service Plan is zone redundant
  name                         = "asp-${local.resource_name}-gateway"
  location                     = azurerm_resource_group.rg_polaris.location
  resource_group_name          = azurerm_resource_group.rg_polaris.name
  os_type                      = "Linux"
  sku_name                     = var.app_service_plan_gateway_sku
  tags                         = local.common_tags
  maximum_elastic_worker_count = 10
}

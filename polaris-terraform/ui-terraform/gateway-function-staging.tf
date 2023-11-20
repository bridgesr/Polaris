#################### Staging1 ######################
resource "azurerm_linux_function_app_slot" "fa_polaris_staging1" {
  name                          = "staging1"
  function_app_id               = azurerm_linux_function_app.fa_polaris.id
  storage_account_name          = azurerm_storage_account.sacpspolaris.name
  storage_account_access_key    = azurerm_storage_account.sacpspolaris.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_gateway_subnet.id
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  tags                          = local.common_tags

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "false"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sacpspolaris.primary_connection_string
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.polaris_sacpspolaris_gateway_staging1_file_share.name
    "WEBSITE_RUN_FROM_PACKAGE"                        = "1"
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/api/status"
    "SCALE_CONTROLLER_LOGGING_ENABLED"                = var.ui_logging.gateway_scale_controller
    "AzureWebJobsStorage"                             = azurerm_storage_account.sacpspolaris.primary_connection_string
    "TenantId"                                        = data.azurerm_client_config.current.tenant_id
    "ClientId"                                        = ""
    "ClientSecret"                                    = ""
    "PolarisPipelineCoordinatorBaseUrl"               = "https://fa-${local.resource_name}-coordinator.azurewebsites.net/api/"
    "PolarisPipelineCoordinatorFunctionAppKey"        = "" //set in deployment script
    "PolarisPipelineCoordinatorDurableExtensionCode"  = "" //set in deployment script
    "BlobServiceUrl"                                  = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobContainerName"                               = "documents"
    "BlobExpirySecs"                                  = 3600
    "BlobUserDelegationKeyExpirySecs"                 = 3600
    "CallingAppValidAudience"                         = var.polaris_webapp_details.valid_audience
    "CallingAppValidScopes"                           = var.polaris_webapp_details.valid_scopes
    "CallingAppValidRoles"                            = var.polaris_webapp_details.valid_roles
    "DdeiBaseUrl"                                     = "https://fa-${local.ddei_resource_name}.azurewebsites.net"
    "DdeiAccessKey"                                   = "" //set in deployment script
  }

  site_config {
    always_on                              = false
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
    cors {
      allowed_origins = [
        "https://as-web-${local.resource_name}.azurewebsites.net",
        "https://as-web-${local.resource_name}-staging1.azurewebsites.net",
        "https://${local.resource_name}-cmsproxy.azurewebsites.net",
        "https://${local.resource_name}-notprod.cps.gov.uk",
        var.env == "dev" ? "http://localhost:3000" : ""
      ]
      support_credentials = true
    }
    vnet_route_all_enabled            = true
    runtime_scale_monitoring_enabled  = true
    elastic_instance_minimum          = 3
    health_check_path                 = "/api/status"
    health_check_eviction_time_in_min = "2"
    application_stack {
      dotnet_version = "6.0"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["FUNCTIONS_EXTENSION_VERSION"],
      app_settings["AzureWebJobsStorage"],
      app_settings["WEBSITE_CONTENTSHARE"],
      app_settings["PolarisPipelineCoordinatorFunctionAppKey"],
      app_settings["PolarisPipelineCoordinatorDurableExtensionCode"],
      app_settings["DdeiAccessKey"],
      app_settings["WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"],
      app_settings["WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"]
    ]
  }
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_gateway_staging1_pe" {
  name                = "${azurerm_linux_function_app.fa_polaris.name}-staging1-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_polaris.name}-staging1-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_polaris.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging1"]
  }
}

#################### Staging2 ######################
resource "azurerm_linux_function_app_slot" "fa_polaris_staging2" {
  name                          = "staging2"
  function_app_id               = azurerm_linux_function_app.fa_polaris.id
  storage_account_name          = azurerm_storage_account.sacpspolaris.name
  storage_account_access_key    = azurerm_storage_account.sacpspolaris.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_gateway_subnet.id
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  tags                          = local.common_tags

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "false"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sacpspolaris.primary_connection_string
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.polaris_sacpspolaris_gateway_staging2_file_share.name
    "WEBSITE_RUN_FROM_PACKAGE"                        = "1"
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/api/status"
    "SCALE_CONTROLLER_LOGGING_ENABLED"                = var.ui_logging.gateway_scale_controller
    "AzureWebJobsStorage"                             = azurerm_storage_account.sacpspolaris.primary_connection_string
    "TenantId"                                        = data.azurerm_client_config.current.tenant_id
    "ClientId"                                        = ""
    "ClientSecret"                                    = ""
    "PolarisPipelineCoordinatorBaseUrl"               = "https://fa-${local.resource_name}-coordinator.azurewebsites.net/api/"
    "PolarisPipelineCoordinatorFunctionAppKey"        = "" //set in deployment script
    "PolarisPipelineCoordinatorDurableExtensionCode"  = "" //set in deployment script
    "BlobServiceUrl"                                  = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobContainerName"                               = "documents"
    "BlobExpirySecs"                                  = 3600
    "BlobUserDelegationKeyExpirySecs"                 = 3600
    "CallingAppValidAudience"                         = var.polaris_webapp_details.valid_audience
    "CallingAppValidScopes"                           = var.polaris_webapp_details.valid_scopes
    "CallingAppValidRoles"                            = var.polaris_webapp_details.valid_roles
    "DdeiBaseUrl"                                     = "https://fa-${local.ddei_resource_name}.azurewebsites.net"
    "DdeiAccessKey"                                   = "" //set in deployment script
  }

  site_config {
    always_on                              = false
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
    cors {
      allowed_origins = [
        "https://as-web-${local.resource_name}.azurewebsites.net",
        "https://as-web-${local.resource_name}-staging2.azurewebsites.net",
        "https://${local.resource_name}-cmsproxy.azurewebsites.net",
        "https://${local.resource_name}-notprod.cps.gov.uk",
        var.env == "dev" ? "http://localhost:3000" : ""
      ]
      support_credentials = true
    }
    vnet_route_all_enabled            = true
    runtime_scale_monitoring_enabled  = true
    elastic_instance_minimum          = 3
    health_check_path                 = "/api/status"
    health_check_eviction_time_in_min = "2"
    application_stack {
      dotnet_version = "6.0"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["FUNCTIONS_EXTENSION_VERSION"],
      app_settings["AzureWebJobsStorage"],
      app_settings["WEBSITE_CONTENTSHARE"],
      app_settings["PolarisPipelineCoordinatorFunctionAppKey"],
      app_settings["PolarisPipelineCoordinatorDurableExtensionCode"],
      app_settings["DdeiAccessKey"],
      app_settings["WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"],
      app_settings["WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"]
    ]
  }
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_gateway_staging2_pe" {
  name                = "${azurerm_linux_function_app.fa_polaris.name}-staging2-pe"
  resource_group_name = azurerm_resource_group.rg_polaris.name
  location            = azurerm_resource_group.rg_polaris.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_polaris.name}-staging2-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_polaris.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging2"]
  }
}
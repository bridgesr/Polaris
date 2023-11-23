#################### Functions #####################
#################### Staging1 ######################
resource "azurerm_windows_function_app_slot" "fa_pdf_generator_staging1" {
  name                          = "staging1"
  function_app_id               = azurerm_windows_function_app.fa_pdf_generator.id
  storage_account_name          = azurerm_storage_account.sa_pdf_generator.name
  storage_account_access_key    = azurerm_storage_account.sa_pdf_generator.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_pdfgenerator_subnet.id
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  tags                          = local.common_tags

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "true"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                        = "1"
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sa_pdf_generator.primary_connection_string
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.pipeline_sa_pdf_generator_file_share_staging1.name
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/api/status"
    "SCALE_CONTROLLER_LOGGING_ENABLED"                = var.pipeline_logging.pdf_generator_scale_controller
    "AzureWebJobsStorage"                             = azurerm_storage_account.sa_pdf_generator.primary_connection_string
    "BlobServiceUrl"                                  = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobServiceContainerName"                        = "documents"
    "HteFeatureFlag"                                  = var.hte_feature_flag
    "ImageConversion__Resolution"                     = var.image_conversion_redaction.resolution
    "ImageConversion__QualityPercent"                 = var.image_conversion_redaction.quality_percent
  }

  site_config {
    ftps_state                       = "FtpsOnly"
    http2_enabled                    = true
    runtime_scale_monitoring_enabled = true
    vnet_route_all_enabled           = true
    elastic_instance_minimum         = 1
    app_scale_limit                  = 3
    application_stack {
      dotnet_version = "v6.0"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  auth_settings {
    enabled                       = false
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "AllowAnonymous"
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["FUNCTIONS_EXTENSION_VERSION"],
      app_settings["AzureWebJobsStorage"],
      app_settings["WEBSITE_CONTENTSHARE"],
      app_settings["WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"],
      app_settings["WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"]
    ]
  }
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_pdf_generator_staging1_pe" {
  name                = "${azurerm_windows_function_app.fa_pdf_generator.name}-staging1-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_windows_function_app.fa_pdf_generator.name}-staging1-psc"
    private_connection_resource_id = azurerm_windows_function_app.fa_pdf_generator.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging1"]
  }
}

#################### Staging2 ######################
resource "azurerm_windows_function_app_slot" "fa_pdf_generator_staging2" {
  name                          = "staging2"
  function_app_id               = azurerm_windows_function_app.fa_pdf_generator.id
  storage_account_name          = azurerm_storage_account.sa_pdf_generator.name
  storage_account_access_key    = azurerm_storage_account.sa_pdf_generator.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_pdfgenerator_subnet.id
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  tags                          = local.common_tags

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet"
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "true"
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "WEBSITE_RUN_FROM_PACKAGE"                        = "1"
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sa_pdf_generator.primary_connection_string
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.pipeline_sa_pdf_generator_file_share_staging2.name
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/api/status"
    "SCALE_CONTROLLER_LOGGING_ENABLED"                = var.pipeline_logging.pdf_generator_scale_controller
    "AzureWebJobsStorage"                             = azurerm_storage_account.sa_pdf_generator.primary_connection_string
    "BlobServiceUrl"                                  = "https://sacps${var.env != "prod" ? var.env : ""}polarispipeline.blob.core.windows.net/"
    "BlobServiceContainerName"                        = "documents"
    "HteFeatureFlag"                                  = var.hte_feature_flag
    "ImageConversion__Resolution"                     = var.image_conversion_redaction.resolution
    "ImageConversion__QualityPercent"                 = var.image_conversion_redaction.quality_percent
  }

  site_config {
    ftps_state                       = "FtpsOnly"
    http2_enabled                    = true
    runtime_scale_monitoring_enabled = true
    vnet_route_all_enabled           = true
    elastic_instance_minimum         = 1
    app_scale_limit                  = 3
    application_stack {
      dotnet_version = "v6.0"
    }
  }

  identity {
    type = "SystemAssigned"
  }

  auth_settings {
    enabled                       = false
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    unauthenticated_client_action = "AllowAnonymous"
  }

  lifecycle {
    ignore_changes = [
      app_settings["WEBSITES_ENABLE_APP_SERVICE_STORAGE"],
      app_settings["WEBSITE_ENABLE_SYNC_UPDATE_SITE"],
      app_settings["FUNCTIONS_EXTENSION_VERSION"],
      app_settings["AzureWebJobsStorage"],
      app_settings["WEBSITE_CONTENTSHARE"],
      app_settings["WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"],
      app_settings["WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"]
    ]
  }
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_pdf_generator_staging2_pe" {
  name                = "${azurerm_windows_function_app.fa_pdf_generator.name}-staging2-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_windows_function_app.fa_pdf_generator.name}-staging2-psc"
    private_connection_resource_id = azurerm_windows_function_app.fa_pdf_generator.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging2"]
  }
}
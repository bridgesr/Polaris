resource "azurerm_linux_function_app_slot" "fa_text_extractor_staging1" {
  name                          = "staging1"
  function_app_id               = azurerm_linux_function_app.fa_text_extractor.id
  storage_account_name          = azurerm_storage_account.sa_text_extractor.name
  storage_account_access_key    = azurerm_storage_account.sa_text_extractor.primary_access_key
  virtual_network_subnet_id     = data.azurerm_subnet.polaris_textextractor_2_subnet.id
  functions_extension_version   = "~4"
  https_only                    = true
  public_network_access_enabled = false
  tags                          = local.common_tags

  app_settings = {
    "AzureWebJobsStorage"                             = azurerm_storage_account.sa_text_extractor.primary_connection_string
    "ComputerVisionClientServiceKey"                  = azurerm_cognitive_account.computer_vision_service.primary_access_key
    "ComputerVisionClientServiceUrl"                  = azurerm_cognitive_account.computer_vision_service.endpoint
    "FUNCTIONS_EXTENSION_VERSION"                     = "~4"
    "FUNCTIONS_WORKER_RUNTIME"                        = "dotnet"
    "HostType"                                        = "Staging1"
    "SCALE_CONTROLLER_LOGGING_ENABLED"                = var.pipeline_logging.text_extractor_scale_controller
    "SearchClientAuthorizationKey"                    = azurerm_search_service.ss.primary_key
    "SearchClientEndpointUrl"                         = "https://${azurerm_search_service.ss.name}.search.windows.net"
    "SearchClientIndexName"                           = jsondecode(file("search-index-definition.json")).name
    "WEBSITE_ADD_SITENAME_BINDINGS_IN_APPHOST_CONFIG" = "1"
    "WEBSITE_CONTENTAZUREFILECONNECTIONSTRING"        = azurerm_storage_account.sa_text_extractor.primary_connection_string
    "WEBSITE_CONTENTOVERVNET"                         = "1"
    "WEBSITE_CONTENTSHARE"                            = azapi_resource.pipeline_sa_text_extractor_file_share_staging1.name
    "WEBSITE_DNS_ALT_SERVER"                          = "168.63.129.16"
    "WEBSITE_DNS_SERVER"                              = var.dns_server
    "WEBSITE_ENABLE_SYNC_UPDATE_SITE"                 = "true"
    "WEBSITE_OVERRIDE_STICKY_DIAGNOSTICS_SETTINGS"    = "0"
    "WEBSITE_OVERRIDE_STICKY_EXTENSION_VERSIONS"      = "0"
    "WEBSITE_RUN_FROM_PACKAGE"                        = "1"
    "WEBSITE_SWAP_WARMUP_PING_PATH"                   = "/api/status"
    "WEBSITES_ENABLE_APP_SERVICE_STORAGE"             = "true"
  }

  site_config {
    ftps_state                             = "FtpsOnly"
    http2_enabled                          = true
    runtime_scale_monitoring_enabled       = true
    vnet_route_all_enabled                 = true
    elastic_instance_minimum               = var.pipeline_component_service_plans.text_extractor_always_ready_instances
    app_scale_limit                        = var.pipeline_component_service_plans.text_extractor_maximum_scale_out_limit
    application_insights_connection_string = data.azurerm_application_insights.global_ai.connection_string
    application_insights_key               = data.azurerm_application_insights.global_ai.instrumentation_key
    application_stack {
      dotnet_version = "6.0"
    }
    health_check_path                 = "/api/status"
    health_check_eviction_time_in_min = "2"
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
      app_settings["WEBSITE_CONTENTSHARE"]
    ]
  }
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "pipeline_text_extractor_staging1_pe" {
  name                = "${azurerm_linux_function_app.fa_text_extractor.name}-staging1-pe"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  subnet_id           = data.azurerm_subnet.polaris_apps2_subnet.id
  tags                = local.common_tags

  private_dns_zone_group {
    name                 = data.azurerm_private_dns_zone.dns_zone_apps.name
    private_dns_zone_ids = [data.azurerm_private_dns_zone.dns_zone_apps.id]
  }

  private_service_connection {
    name                           = "${azurerm_linux_function_app.fa_text_extractor.name}-staging1-psc"
    private_connection_resource_id = azurerm_linux_function_app.fa_text_extractor.id
    is_manual_connection           = false
    subresource_names              = ["sites-staging1"]
  }
}
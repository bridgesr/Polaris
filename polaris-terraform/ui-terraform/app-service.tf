#################### App Service ####################

resource "azurerm_linux_web_app" "as_web_polaris" {
  name                = "as-web-${local.resource_name}"
  location            = azurerm_resource_group.rg_polaris.location
  resource_group_name = azurerm_resource_group.rg_polaris.name
  service_plan_id     = azurerm_service_plan.asp_polaris.id
  https_only          = true

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY"  = azurerm_application_insights.ai_polaris.instrumentation_key
    "REACT_APP_CLIENT_ID"             = module.azurerm_app_reg_as_web_polaris.client_id
    "REACT_APP_TENANT_ID"             = data.azurerm_client_config.current.tenant_id
    "REACT_APP_GATEWAY_BASE_URL"      = "https://${azurerm_linux_function_app.fa_polaris.name}.azurewebsites.net"
    "REACT_APP_GATEWAY_SCOPE"         = "https://CPSGOVUK.onmicrosoft.com/${azurerm_linux_function_app.fa_polaris.name}/user_impersonation"
  }

  site_config {
    app_command_line = "node subsititute-config.js; npx serve -s"
    linux_fx_version = "NODE|14-lts"
  }

  tags = {
    environment = var.environment_tag
  }

  auth_settings {
    enabled                       = true
    issuer                        = "https://sts.windows.net/${data.azurerm_client_config.current.tenant_id}/"
    
    # AllowAnonymous as no need for web auth if we are hosted within CPS network, the SPA auth handles hiding UI 
    # from unauthed users. Also having web auth switched on means that Cypress automation tests don't work.
    unauthenticated_client_action = "AllowAnonymous"
    token_store_enabled           = true
    /*active_directory {
      client_id         = module.azurerm_app_reg_as_web_polaris.client_id
      client_secret     = azuread_application_password.asap_web_polaris_app_service.value
      allowed_audiences = ["https://CPSGOVUK.onmicrosoft.com/as-web-${local.resource_name}"]
    }*/
  }
}

module "azurerm_app_reg_as_web_polaris" {
  source  = "./modules/terraform-azurerm-azuread-app-registration"
  display_name = "as-web-${local.resource_name}-appreg"
  identifier_uris = ["https://CPSGOVUK.onmicrosoft.com/as-web-${local.resource_name}"]
  owners          = [data.azuread_service_principal.terraform_service_principal.object_id]
  prevent_duplicate_names = true
  #use this code for adding api permissions
  required_resource_access = [{
      # Microsoft Graph
      resource_app_id = "00000003-0000-0000-c000-000000000000"
      resource_access = [{
        id   = "311a71cc-e848-46a1-bdf8-97ff7156d8e6" # read user
        type = "Scope"
      }]
    },
    {
      resource_app_id = module.azurerm_app_reg_fa_polaris.client_id
      resource_access = [{
        id   = module.azurerm_app_reg_fa_polaris.oauth2_permission_scope_ids["user_impersonation"]
        type = "Scope"
      }]
    }]
  single_page_application = {
    redirect_uris = var.env != "prod" ? ["https://as-web-${local.resource_name}.azurewebsites.net/", "http://localhost:3000/"] : ["https://as-web-${local.resource_name}.azurewebsites.net/"]
  }
  web = {
    homepage_url  = "https://as-web-${local.resource_name}.azurewebsites.net"
    redirect_uris = ["https://getpostman.com/oauth2/callback"]
    implicit_grant = {
      access_token_issuance_enabled = true
    }
  }
  tags = ["as-web-${local.resource_name}-appreg", "terraform"]
}

resource "azuread_application_password" "asap_web_polaris_app_service" {
  application_object_id = module.azurerm_app_reg_as_web_polaris.object_id
  end_date_relative     = "17520h"
}

module "azurerm_service_principal_sp_polaris_web" {
  source         = "./modules/terraform-azurerm-azuread_service_principal"
  application_id = module.azurerm_app_reg_as_web_polaris.client_id
  app_role_assignment_required = false
  owners         = [data.azurerm_client_config.current.object_id]
  depends_on = [module.azurerm_app_reg_as_web_polaris]
}

resource "azuread_service_principal_password" "sp_polaris_web_pw" {
  service_principal_id = module.azurerm_service_principal_sp_polaris_web.object_id
  depends_on = [module.azurerm_service_principal_sp_polaris_web]
}

resource "azuread_application_pre_authorized" "fapre_polaris_web" {
  application_object_id = module.azurerm_app_reg_fa_polaris.object_id
  authorized_app_id     = module.azurerm_app_reg_as_web_polaris.client_id
  permission_ids        = [module.azurerm_app_reg_fa_polaris.oauth2_permission_scope_ids["user_impersonation"]]
  depends_on = [module.azurerm_app_reg_fa_polaris, module.azurerm_app_reg_as_web_polaris]
}

resource "azuread_service_principal_delegated_permission_grant" "polaris_web_grant_access_to_msgraph" {
  service_principal_object_id          = module.azurerm_service_principal_sp_polaris_web.object_id
  resource_service_principal_object_id = azuread_service_principal.msgraph.object_id
  claim_values                         = ["User.Read"]
  depends_on = [module.azurerm_service_principal_sp_polaris_web, azuread_service_principal.msgraph]
}
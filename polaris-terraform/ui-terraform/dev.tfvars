env                          = "dev"
location                     = "UK South"
environment_tag              = "development"
app_service_plan_web_sku     = "P1v2"
app_service_plan_gateway_sku = "EP1"
app_service_plan_proxy_sku   = "P1v2"
dns_server                   = "10.7.197.20"

polaris_webapp_details = {
  valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-dev-gateway"
  valid_scopes   = "user_impersonation"
  valid_roles    = ""
}

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Development"

certificate_name    = "polaris-dev-notprod3536a9f3-a9a0-48b4-9b40-8c76083cad2e"
custom_domain_name  = "polaris-dev-notprod.cps.gov.uk"

ui_logging = {
  gateway_scale_controller       = "AppInsights:Verbose"
  auth_handover_scale_controller = "AppInsights:Verbose"
  proxy_scale_controller         = "AppInsights:Verbose"
}

cms_details = {
  upstream_cms_ip                 = "10.2.177.14"
  upstream_cms_modern_ip          = "10.2.177.55"
  upstream_cms_domain_name        = "cin3.cps.gov.uk"
  upstream_cms_modern_domain_name = "cmsmodcin3.cps.gov.uk"
}
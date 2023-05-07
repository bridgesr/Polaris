env                  = "prod"
environment_tag      = "production"
app_service_plan_sku = "EP1"
dns_server           = "10.7.204.164"

terraform_service_principal_display_name = "Azure Pipeline: Innovation-Production"

pipeline_logging = {
  coordinator_scale_controller    = "AppInsights:None"
  pdf_generator_scale_controller  = "AppInsights:None"
  text_extractor_scale_controller = "AppInsights:None"
}
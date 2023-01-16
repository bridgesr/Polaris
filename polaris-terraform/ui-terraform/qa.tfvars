env = "qa"
location = "UK South"
environment_tag="QA"

app_service_plan_sku = {
    size = "B1"
    tier = "Basic"
}

polaris_webapp_details = {
    valid_audience = "https://CPSGOVUK.onmicrosoft.com/fa-polaris-qa-gateway"
    valid_scopes = "user_impersonation"
    valid_roles = ""
}
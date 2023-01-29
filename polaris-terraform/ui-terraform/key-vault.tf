#################### Key Vault ####################

resource "azurerm_key_vault" "kv_polaris" {
  name                = "kv-${local.resource_name}"
  location            = azurerm_resource_group.rg_polaris.location
  resource_group_name = azurerm_resource_group.rg_polaris.name
  tenant_id           = data.azurerm_client_config.current.tenant_id

  sku_name = "standard"

  network_acls {
    default_action = "Deny"
    bypass         = "AzureServices"
    virtual_network_subnet_ids = [
      data.azurerm_subnet.polaris_ci_subnet.id,
      data.azurerm_subnet.polaris_gateway_subnet.id
    ]
  }
}

# Create Private Endpoint
resource "azurerm_private_endpoint" "polaris_key_vault_pe" {
  name                  = "${azurerm_key_vault.kv_polaris.name}-pe"
  resource_group_name   = azurerm_resource_group.rg_polaris.name
  location              = azurerm_resource_group.rg_polaris.location
  subnet_id             = data.azurerm_subnet.polaris_apps_subnet.id

  private_service_connection {
    name                           = "${azurerm_key_vault.kv_polaris.name}-psc"
    private_connection_resource_id = azurerm_key_vault.kv_polaris.id
    is_manual_connection           = false
    subresource_names              = ["vault"]
  }
}

# Create DNS A Record
resource "azurerm_private_dns_a_record" "polaris_key_vault_dns_a" {
  name                = azurerm_key_vault.kv_polaris.name
  zone_name           = data.azurerm_private_dns_zone.dns_zone_keyvault.name
  resource_group_name = "rg-${var.networking_resource_name_suffix}"
  ttl                 = 300
  records             = [azurerm_private_endpoint.polaris_key_vault_pe.private_service_connection.0.private_ip_address]
}

resource "azurerm_key_vault_access_policy" "kvap_fa_polaris_gateway" {
  key_vault_id = azurerm_key_vault.kv_polaris.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = azurerm_linux_function_app.fa_polaris.identity[0].principal_id

  secret_permissions = [
    "Get",
  ]
}

resource "azurerm_key_vault_access_policy" "kvap_terraform_sp" {
  key_vault_id = azurerm_key_vault.kv_polaris.id
  tenant_id    = data.azurerm_client_config.current.tenant_id
  object_id    = data.azuread_service_principal.terraform_service_principal.object_id

  secret_permissions = [
    "Get",
    "Set",
    "Delete",
    "Purge"
  ]
}

resource "azurerm_key_vault_secret" "kvs_fa_polaris_client_secret" {
  name         = "PolarisFunctionAppRegistrationClientSecret"
  value        = azuread_application_password.faap_polaris_app_service.value
  key_vault_id = azurerm_key_vault.kv_polaris.id
  depends_on = [
    azurerm_key_vault_access_policy.kvap_terraform_sp
  ]
}

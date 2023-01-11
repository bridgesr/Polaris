resource "azurerm_storage_account" "sa" {
  name                = "sacps${var.env != "prod" ? var.env : ""}polarispipeline"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location

  account_kind              = "StorageV2"
  account_replication_type  = "LRS"
  account_tier              = "Standard"
  enable_https_traffic_only = true

  min_tls_version = "TLS1_2"

  network_rules {
    default_action = "Allow"
  }

  identity {
    type = "SystemAssigned"
  }
}

resource "azurerm_storage_account_customer_managed_key" "polaris_storage_pipeline_cmk" {
  storage_account_id = azurerm_storage_account.sa.id
  key_vault_id       = azurerm_key_vault.kv.id
  key_name           = azurerm_key_vault_key.kvap_sa_customer_managed_key.name

  depends_on = [
    azurerm_role_assignment.kv_role_sa_kvcseu,
    azurerm_key_vault_key.kvap_sa_customer_managed_key,
    azurerm_storage_account.sa,
    azurerm_role_assignment.kv_role_terraform_sp
  ]
}

resource "azurerm_storage_container" "container" {
  name                  = "documents"
  storage_account_name  = azurerm_storage_account.sa.name
  container_access_type = "private"
}

resource "azurerm_storage_management_policy" "pipeline-documents-lifecycle" {
  storage_account_id   = azurerm_storage_account.sa.id
  
  rule {
    name               = "polaris-documents-${var.env != "prod" ? var.env : ""}-lifecycle"
    enabled            = true
    filters {
      prefix_match     = ["documents"]
      blob_types       = ["blockBlob"]
    }
    actions {
      base_blob {
        delete_after_days_since_modification_greater_than = 7
      }
    }
  }
  depends_on = [azurerm_storage_account.sa]
}

data "azurerm_function_app_host_keys" "fa_text_extractor_generator_host_keys" {
  name                = "fa-${local.resource_name}-text-extractor"
  resource_group_name = azurerm_resource_group.rg.name
  
  depends_on = [azurerm_linux_function_app.fa_text_extractor]
}

resource "azurerm_eventgrid_system_topic" "pipeline_document_deleted_topic" {
  name                   = "pipeline-document-deleted-${var.env != "prod" ? var.env : ""}-topic"
  location               = azurerm_resource_group.rg.location
  resource_group_name    = azurerm_resource_group.rg.name
  source_arm_resource_id = azurerm_storage_account.sa.id
  topic_type             = "Microsoft.Storage.StorageAccounts"
  
  tags = {
    environment = "${var.env}"
  }
  depends_on = [azurerm_storage_account.sa,azurerm_storage_management_policy.pipeline-documents-lifecycle]
}

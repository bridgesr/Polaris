resource "azurerm_eventgrid_system_topic_event_subscription" "pipeline_document_deleted_event_subscription" {
  name                   = "pipeline-storage-document-deleted-${var.env != "prod" ? var.env : ""}-event-sub"
  system_topic           = data.azurerm_eventgrid_system_topic.pipeline_document_deleted_topic.name
  resource_group_name    = "rg-${local.pipeline_resource_name}"

  azure_function_endpoint {
    function_id          = "${data.azurerm_linux_function_app.fa_text_extractor.id}/functions/HandleDocumentDeletedEvent"
  }

  included_event_types = ["Blob Deleted"]
}

resource "azurerm_role_assignment" "pdf_generator_blob_data_contributor" {
  scope                = data.azurerm_storage_container.pipeline_storage_container.resource_manager_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = data.azurerm_windows_function_app.fa_pdf_generator.identity[0].principal_id
}

resource "azurerm_role_assignment" "text_extractor_blob_contributor" {
  scope                = data.azurerm_storage_container.pipeline_storage_container.resource_manager_id
  role_definition_name = "Storage Blob Data Reader"
  principal_id         = data.azurerm_linux_function_app.fa_text_extractor.identity[0].principal_id
}

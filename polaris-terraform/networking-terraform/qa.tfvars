environment = {
  name  = "qa"
  alias = "qa"
}

vnetAddressSpace = "10.7.198.0/23"
vnetDnsServer    = "10.7.198.164"

cmsServicesSubnet                  = "10.7.198.0/28"
ddeiServicesSubnet                 = "10.7.198.32/27"
polarisPipelineSaSubnet            = "10.7.198.96/28"
polarisPipelineCoordinatorSubnet   = "10.7.198.208/28"
polarisPipelinePdfGeneratorSubnet  = "10.7.198.80/28"
polarisPipelineTextExtractorSubnet = "10.7.199.0/28"
polarisPipelineKeyVaultSubnet      = "10.7.198.64/29"
polarisGatewaySubnet               = "10.7.198.192/28"
polarisUiSubnet                    = "10.7.198.16/28"
polarisProxySubnet                 = "10.7.198.112/28"
polarisAppsSubnet                  = "10.7.198.224/27"
polarisCiSubnet                    = "10.7.198.176/28"
polarisDnsResolveSubnet            = "10.7.198.160/28"
gatewaySubnet                      = "10.7.198.128/27"
polarisAuthHandoverSubnet          = "10.7.199.16/28"
mockCmsServiceSubnet               = "10.7.199.48/28"
polarisAmplsSubnet                 = "10.7.199.64/27"
polarisPipelineNetheriteSubnet     = "10.7.199.96/27"
polarisPipelineSa2Subnet           = "10.7.199.128/27"

terraform_service_principal_display_name = "Azure Pipeline: Innovation-QA"
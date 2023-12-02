param name string
param location string = resourceGroup().location
param tags object
param openAIServiceEndpoint string
param openAIChatGptDeployment string
param openAIAzureStorageAccountEndpoint string
param openAIAzureStorageContainer string
param openAIAzureSearchServiceEndpoint string
param openAIAzureSearchIndex string
param botIdentityId string
param botClientId string
param botTenantId string

resource appServicePlan 'Microsoft.Web/serverfarms@2022-09-01' = {
    name: 'plan-${name}'
    location: location
    tags: tags
    properties: {
        reserved: true
    }
    sku: {
        name: 'S1'
        tier: 'Standard'
    }
    kind: 'linux'
}

resource appService 'Microsoft.Web/sites@2022-09-01' = {
    name: name
    location: location
    tags: union(tags, { 'azd-service-name': 'web' })
    properties: {
        serverFarmId: appServicePlan.id
        siteConfig: {
            linuxFxVersion: 'DOTNETCORE|8.0'
            appSettings: [
                {
                    name: 'AzureOpenAIConfig__ServiceEndpoint'
                    value: openAIServiceEndpoint
                }
                {
                    name: 'AzureOpenAIConfig__ChatGptDeployment'
                    value: openAIChatGptDeployment
                }
                {
                    name: 'AzureOpenAIConfig__AzureStorageAccountEndpoint'
                    value: openAIAzureStorageAccountEndpoint
                }
                {
                    name: 'AzureOpenAIConfig__AzureStorageContainer'
                    value: openAIAzureStorageContainer
                }
                {
                    name: 'AzureOpenAIConfig__AzureSearchServiceEndpoint'
                    value: openAIAzureSearchServiceEndpoint
                }
                {
                    name: 'AzureOpenAIConfig__AzureSearchIndex'
                    value: openAIAzureSearchIndex
                }
                {
                    name: 'AzureOpenAIConfig__EmbeddingDeployment'
                    value: 'embedding'
                }
                {
                    name: 'MicrosoftAppId'
                    value: botClientId
                }
                {
                    name: 'MicrosoftAppTenantId'
                    value: botTenantId
                }
                {
                    name: 'MicrosoftAppType'
                    value: 'UserAssignedMSI'
                }
            ]
        }
    }
    identity: {
        type: 'SystemAssigned, UserAssigned'
        userAssignedIdentities: {
            '${botIdentityId}': {}
        }
    }
}

output appServiceName string = appService.name
output appServiceUri string = appService.properties.defaultHostName
output principalId string = appService.identity.principalId

targetScope = 'subscription'

@minLength(1)
@maxLength(64)
@description('Name of the the environment which is used to generate a short unique hash used in all resources.')
param environmentName string

@minLength(1)
@description('Primary location for all resources')
param location string

@description('Id of the user or app to assign application roles')
param principalId string = ''
param principalType string = 'User'

param resourceGroupName string = 'demo-openai'
param solutionName string = 'smartbot'

param searchServiceResourceGroupName string = ''
param searchServiceSkuName string = 'standard'
param searchIndexName string = 'gptkbindex'

param storageResourceGroupName string = ''
param storageContainerName string = 'content'

param openAiResourceGroupName string = ''
param openAiSkuName string = 'S0'

param formRecognizerResourceGroupName string = ''

param chatGptDeploymentName string = 'chat'
param chatGptModelName string = 'gpt-35-turbo'

var abbrs = loadJsonContent('./abbreviations.json')
var suffix = take(toLower(uniqueString(environmentName, subscription().id, location)), 4)

param tags string = ''
var baseTags = { 'azd-env-name': environmentName }
var updatedTags = union(empty(tags) ? {} : base64ToJson(tags), baseTags)

// Organize resources in a resource group
resource resourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' = {
    name: resourceGroupName
    location: location
    tags: updatedTags
}

resource openAiResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!empty(openAiResourceGroupName)) {
    name: !empty(openAiResourceGroupName) ? openAiResourceGroupName : resourceGroup.name
}

resource formRecognizerResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!empty(formRecognizerResourceGroupName)) {
    name: !empty(formRecognizerResourceGroupName) ? formRecognizerResourceGroupName : resourceGroup.name
}

resource searchServiceResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!empty(searchServiceResourceGroupName)) {
    name: !empty(searchServiceResourceGroupName) ? searchServiceResourceGroupName : resourceGroup.name
}

resource storageResourceGroup 'Microsoft.Resources/resourceGroups@2021-04-01' existing = if (!empty(storageResourceGroupName)) {
    name: !empty(storageResourceGroupName) ? storageResourceGroupName : resourceGroup.name
}

// managed identity (service principal) 
module botIdentity 'core/security/idenity.bicep' = {
    name: 'bot-identity'
    scope: resourceGroup
    params: {
        name: '${abbrs.managedIdentityUserAssignedIdentities}${solutionName}-${suffix}'
        location: location
        tags: updatedTags
    }
}

// Web api
module web './app/web-service.bicep' = {
    name: 'web'
    scope: resourceGroup
    params: {
        name: '${abbrs.webSitesAppService}${solutionName}-${suffix}'
        location: location
        tags: updatedTags
        openAIServiceEndpoint: openAi.outputs.endpoint
        openAIChatGptDeployment: chatGptDeploymentName
        openAIAzureStorageAccountEndpoint: storage.outputs.primaryEndpoints.blob
        openAIAzureStorageContainer: storageContainerName
        openAIAzureSearchServiceEndpoint: searchService.outputs.endpoint
        openAIAzureSearchIndex: searchIndexName
        botIdentityId: botIdentity.outputs.id
        botTenantId: botIdentity.outputs.tenantId
        botClientId: botIdentity.outputs.clientId
    }
}

module bot 'app/bot-service.bicep' = {
    name: 'bot'
    scope: resourceGroup
    params: {
        name: '${solutionName}-${suffix}'
        displayName: solutionName
        botSku: 'F0'
        tags: updatedTags
        appServiceUri: web.outputs.appServiceUri
        managedIdentityClientId: botIdentity.outputs.clientId
        managedIdentityTenantId: botIdentity.outputs.tenantId
        managedIdentityId: botIdentity.outputs.id
    }
}

module openAi 'core/ai/cognitive-service.bicep' = {
    name: 'openai'
    scope: openAiResourceGroup
    params: {
        name: '${abbrs.cognitiveServicesAccounts}${solutionName}'
        location: location
        tags: updatedTags
        skuName: openAiSkuName
        chatGptDeploymentName: chatGptDeploymentName
        chatGptModelName: chatGptModelName
    }
}

module searchService 'core/search/search-service.bicep' = {
    name: 'search-service'
    scope: searchServiceResourceGroup
    params: {
        name: '${abbrs.searchSearchServices}${solutionName}-${suffix}'
        location: location
        tags: updatedTags
        authOptions: {
            aadOrApiKey: {
                aadAuthFailureMode: 'http401WithBearerChallenge'
            }
        }
        sku: {
            name: searchServiceSkuName
        }
        semanticSearch: 'free'
    }
}

module storage 'core/storage/storage-account.bicep' = {
    name: 'storage'
    scope: storageResourceGroup
    params: {
        name: '${abbrs.storageStorageAccounts}${solutionName}${suffix}'
        location: location
        tags: updatedTags
        publicNetworkAccess: 'Enabled'
        sku: {
            name: 'Standard_ZRS'
        }
        deleteRetentionPolicy: {
            enabled: true
            days: 2
        }
        containers: [
            {
                name: storageContainerName
                publicAccess: 'Blob'
            }
        ]
    }
}

// USER ROLES FOR UPLOADING CONTENT
module openAiRoleUser 'core/security/role.bicep' = {
    scope: openAiResourceGroup
    name: 'openai-role-user'
    params: {
      principalId: principalId
      roleDefinitionId: '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
      principalType: principalType
    }
  }
  
  module formRecognizerRoleUser 'core/security/role.bicep' = {
    scope: formRecognizerResourceGroup
    name: 'formrecognizer-role-user'
    params: {
      principalId: principalId
      roleDefinitionId: 'a97b65f3-24c7-4388-baec-2e87135dc908'
      principalType: principalType
    }
  }
  
  module storageRoleUser 'core/security/role.bicep' = {
    scope: storageResourceGroup
    name: 'storage-role-user'
    params: {
      principalId: principalId
      roleDefinitionId: '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
      principalType: principalType
    }
  }
  
  module storageContribRoleUser 'core/security/role.bicep' = {
    scope: storageResourceGroup
    name: 'storage-contribrole-user'
    params: {
      principalId: principalId
      roleDefinitionId: 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'
      principalType: principalType
    }
  }
  
  module searchRoleUser 'core/security/role.bicep' = {
    scope: searchServiceResourceGroup
    name: 'search-role-user'
    params: {
      principalId: principalId
      roleDefinitionId: '1407120a-92aa-4202-b7e9-c0e197c71c8f'
      principalType: principalType
    }
  }
  
  module searchContribRoleUser 'core/security/role.bicep' = {
    scope: searchServiceResourceGroup
    name: 'search-contrib-role-user'
    params: {
      principalId: principalId
      roleDefinitionId: '8ebe5a00-799e-43f5-93ac-243d3dce84a7'
      principalType: principalType
    }
  }
  
  module searchSvcContribRoleUser 'core/security/role.bicep' = {
    scope: searchServiceResourceGroup
    name: 'search-svccontrib-role-user'
    params: {
      principalId: principalId
      roleDefinitionId: '7ca78c08-252a-4471-8644-bb5ff32d4ba0'
      principalType: principalType
    }
  }

// SYSTEM IDENTITIES
module openAiRoleBackend 'core/security/role.bicep' = {
    scope: openAiResourceGroup
    name: 'openai-role-backend'
    params: {
        principalId: web.outputs.principalId
        roleDefinitionId: '5e0bd9bd-7b93-4f28-af87-19fc36ad61bd'
        principalType: 'ServicePrincipal'
    }
}

module storageRoleBackend 'core/security/role.bicep' = {
    scope: storageResourceGroup
    name: 'storage-role-backend'
    params: {
        principalId: web.outputs.principalId
        roleDefinitionId: '2a2b9908-6ea1-4ae2-8e65-a410df84e7d1'
        principalType: 'ServicePrincipal'
    }
}

module searchRoleBackend 'core/security/role.bicep' = {
    scope: searchServiceResourceGroup
    name: 'search-role-backend'
    params: {
        principalId: web.outputs.principalId
        roleDefinitionId: '1407120a-92aa-4202-b7e9-c0e197c71c8f'
        principalType: 'ServicePrincipal'
    }
}

output AZURE_LOCATION string = location
output AZURE_TENANT_ID string = tenant().tenantId
output AZURE_RESOURCE_GROUP string = resourceGroup.name
output AZURE_SOLUTION_NAME string = solutionName

output AZURE_OPENAI_SERVICE string = openAi.outputs.name
output AZURE_OPENAI_ENDPOINT string = openAi.outputs.endpoint
output AZURE_OPENAI_RESOURCE_GROUP string = openAiResourceGroup.name
output AZURE_OPENAI_CHATGPT_DEPLOYMENT string = chatGptDeploymentName

output AZURE_FORMRECOGNIZER_SERVICE string = openAi.outputs.formRecognizerName
output AZURE_FORMRECOGNIZER_SERVICE_ENDPOINT string = openAi.outputs.formRecognizerEndpoint
output AZURE_FORMRECOGNIZER_RESOURCE_GROUP string = formRecognizerResourceGroup.name

output AZURE_SEARCH_INDEX string = searchIndexName
output AZURE_SEARCH_SERVICE string = searchService.outputs.name
output AZURE_SEARCH_SERVICE_RESOURCE_GROUP string = searchServiceResourceGroup.name
output AZURE_SEARCH_SERVICE_ENDPOINT string = searchService.outputs.endpoint

output AZURE_STORAGE_ACCOUNT string = storage.outputs.name
output AZURE_STORAGE_CONTAINER string = storageContainerName
output AZURE_STORAGE_RESOURCE_GROUP string = storageResourceGroup.name
output AZURE_STORAGE_BLOB_ENDPOINT string = storage.outputs.primaryEndpoints.blob

output SERVICE_WEB_NAME string = web.outputs.appServiceName
output AZURE_BOT_SERVICE_APP_ID string = botIdentity.outputs.principalId

param name string
param displayName string
param botSku string
param managedIdentityId string
param managedIdentityClientId string
param managedIdentityTenantId string
param appServiceUri string
param tags object

resource botService 'Microsoft.BotService/botServices@2022-09-15' = {
    location: 'global'
    name: name
    kind: 'azurebot'
    tags: tags
    sku: {
        name: botSku
    }
    properties: {
        displayName: displayName
        msaAppMSIResourceId: managedIdentityId
        msaAppId: managedIdentityClientId
        msaAppTenantId: managedIdentityTenantId
        msaAppType: 'UserAssignedMSI'
        endpoint: 'https://${appServiceUri}/api/messages'
        schemaTransformationVersion: '1.3'
        disableLocalAuth: false
        isStreamingSupported: false
        publishingCredentials: null
    }
}

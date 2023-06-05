param name string
param location string 
param tags object = {}
@description('The custom subdomain name used to access the API. Defaults to the value of the name parameter.')
param customSubDomainName string = name
param skuName string
param chatGptDeploymentName string
param chatGptModelName string
param publicNetworkAccess string = 'Enabled'
param sku object = {
  name: skuName
}

resource account 'Microsoft.CognitiveServices/accounts@2022-10-01' = {
  name: name
  location: location
  tags: tags
  kind: 'OpenAI'
  properties: {
    customSubDomainName: customSubDomainName
    publicNetworkAccess: publicNetworkAccess
  }
  sku: sku  
}

resource formRecognizer 'Microsoft.CognitiveServices/accounts@2022-10-01' = {
  name: 'formrecognizer'
  location: location
  tags: tags
  kind: 'FormRecognizer'
  properties: {
    customSubDomainName: customSubDomainName
    publicNetworkAccess: publicNetworkAccess
  }
  sku: sku
}

resource deployment 'Microsoft.CognitiveServices/accounts/deployments@2022-10-01' = {
  parent: account
  name: chatGptDeploymentName
  properties: {
    model: {
      format: 'OpenAI'
      name: chatGptModelName
      version: '0301'  
    }
    scaleSettings: {
      scaleType: 'Standard'
    }
  }
}

output id string = account.id
output endpoint string = account.properties.endpoint
output name string = account.name
output formRecognizerEndpoint string = formRecognizer.properties.endpoint
output formRecognizerName string = formRecognizer.name

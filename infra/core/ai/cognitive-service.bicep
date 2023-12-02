param name string
param location string 
param tags object = {}
@description('The custom subdomain name used to access the API. Defaults to the value of the name parameter.')
param customSubDomainName string = name
param skuName string
param chatGptDeploymentName string
param chatGptModelName string
param embedDeploymentName string
param embedModelName string
param publicNetworkAccess string = 'Enabled'
param sku object = {
  name: skuName
}

resource account 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: name
  location: location
  tags: tags
  kind: 'OpenAI'
  properties: {
    customSubDomainName: customSubDomainName
    publicNetworkAccess: publicNetworkAccess
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
  sku: sku  
}

resource formRecognizer 'Microsoft.CognitiveServices/accounts@2023-05-01' = {
  name: 'formrecognizer'
  location: location
  tags: tags
  kind: 'FormRecognizer'
  properties: {
    customSubDomainName: customSubDomainName
    publicNetworkAccess: publicNetworkAccess
    networkAcls: {
      defaultAction: 'Allow'
    }
  }
  sku: sku
}

resource deploymentChat 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: account
  name: chatGptDeploymentName
  properties: {
    model: {
      format: 'OpenAI'
      name: chatGptModelName
      version: '0613'  
    }
  }
  sku: {
    name: 'Standard'
    capacity: 30
  }
}

resource deploymentEmbed 'Microsoft.CognitiveServices/accounts/deployments@2023-05-01' = {
  parent: account
  name: embedDeploymentName
  properties: {
    model: {
      format: 'OpenAI'
      name: embedModelName
      version: '2'  
    }
  }
  sku: {
    name: 'Standard'
    capacity: 30
  }
}


output id string = account.id
output endpoint string = account.properties.endpoint
output name string = account.name
output formRecognizerEndpoint string = formRecognizer.properties.endpoint
output formRecognizerName string = formRecognizer.name

namespace BotBuilderOpenAi;

public class OpenAIConfig
{
    public string ServiceEndpoint { get; set; } = default!;
    public string ChatGptDeployment { get; set; } = default!;
    public string EmbeddingDeployment { get; set; } = default!;  
    public string AzureStorageAccountEndpoint { get; set; } = default!;
    public string AzureStorageContainer { get; set; } = default!;
    public string AzureSearchServiceEndpoint { get; set; } = default!;
    public string AzureSearchIndex { get; set; } = default!;
}

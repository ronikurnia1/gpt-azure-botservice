namespace BotBuilderOpenAi.Extensions;

internal static class OpenAIConfigExtensions
{
    internal static string ToCitationBaseUrl(this OpenAIConfig config)
    {
        var endpoint = config.AzureStorageAccountEndpoint;
        ArgumentException.ThrowIfNullOrEmpty(endpoint);

        var builder = new UriBuilder(endpoint)
        {
            Path = config.AzureStorageContainer
        };

        return builder.Uri.AbsoluteUri;
    }
}


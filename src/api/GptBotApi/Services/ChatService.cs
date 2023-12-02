using Azure.AI.OpenAI;
using Azure.Search.Documents;
using BotBuilderOpenAi;
using BotBuilderOpenAi.Models;
using BotBuilderOpenAi.Services;
using Microsoft.Extensions.Options;

namespace GptBotApi.Services;

public interface IChatService
{
    public Task<Response> GetResponse(ChatRequest request);
}

public class ChatService(IOptions<OpenAIConfig> options, SearchClient searchClient, OpenAIClient openAIClient) : IChatService
{
    private readonly OpenAIBotService chatService = new(searchClient, openAIClient, options.Value);

    public async Task<Response> GetResponse(ChatRequest chatRequest)
    {
        return await chatService.ReplyAsync(chatRequest.History, chatRequest.Overrides);
    }
}

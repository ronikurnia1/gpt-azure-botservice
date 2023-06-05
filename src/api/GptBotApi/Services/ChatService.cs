using Azure.Search.Documents;
using BotBuilderOpenAi;
using BotBuilderOpenAi.Models;
using BotBuilderOpenAi.Services;
using GptBotApi.Extensions;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Options;

namespace GptBotApi.Services;

public interface IChatService
{
    public Task<Response> GetResponse(ChatRequest request);
}

public class ChatService : IChatService
{
    private readonly OpenAIBotService chatService;

    public ChatService(IOptions<OpenAIConfig> options, SearchClient searchClient, 
        AzureOpenAIChatCompletionService completionService)
    {
        chatService = new OpenAIBotService(searchClient, completionService, options.Value);
    }

    public async Task<Response> GetResponse(ChatRequest chatRequest)
    {
        return await chatService.ReplyAsync(chatRequest.History, chatRequest.Overrides);
    }
}

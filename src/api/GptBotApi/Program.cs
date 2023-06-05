
using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Search.Documents;
using BotBuilderOpenAi;
using BotBuilderOpenAi.Services;
using GptBotApi;
using GptBotApi.Bot;
using GptBotApi.Dialogs;
using GptBotApi.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<OpenAIConfig>(builder.Configuration.GetSection("AzureOpenAIConfig"));

builder.Services.AddHttpClient().AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.MaxDepth = HttpHelper.BotMessageSerializerSettings.MaxDepth;
});


// Create the Bot Framework Authentication to be used with the Bot Adapter.
builder.Services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();

// Create the Bot Adapter with error handling enabled.
builder.Services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

var storage = new MemoryStorage();

// Create the User state passing in the storage layer.
builder.Services.AddSingleton(new UserState(storage));

// Create the Conversation state passing in the storage layer.
builder.Services.AddSingleton(new ConversationState(storage));

builder.Services.AddSingleton<MainDialog>();

// Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
builder.Services.AddSingleton<IBot, ChatBot<MainDialog>>();

// SearchClient
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IOptions<OpenAIConfig>>().Value;
    var (azureSearchServiceEndpoint, azureSearchIndex) =
        (config.AzureSearchServiceEndpoint, config.AzureSearchIndex);

    ArgumentException.ThrowIfNullOrEmpty(azureSearchServiceEndpoint);

    var searchClient = new SearchClient(
        new Uri(azureSearchServiceEndpoint), azureSearchIndex, new DefaultAzureCredential());

    return searchClient;
});

// OpenAIClient
builder.Services.AddSingleton(sp =>
{
    var config = sp.GetRequiredService<IOptions<OpenAIConfig>>().Value;
    var azureOpenAiServiceEndpoint = config.ServiceEndpoint;

    ArgumentException.ThrowIfNullOrEmpty(azureOpenAiServiceEndpoint);

    var openAIClient = new OpenAIClient(
        new Uri(azureOpenAiServiceEndpoint), new DefaultAzureCredential());

    return openAIClient;
});

builder.Services.AddSingleton<AzureOpenAIChatCompletionService>();
builder.Services.AddSingleton<IChatService, ChatService>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseDefaultFiles()
    .UseStaticFiles()
    .UseWebSockets()
    .UseRouting()
    .UseAuthorization()
    .UseEndpoints(endpoints =>
    {
        endpoints.MapControllers();
    });

app.Run();

// Copyright (c) Microsoft. All rights reserved.

using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.AI.TextCompletion;

namespace BotBuilderOpenAi.Services;

public sealed class AzureOpenAIChatCompletionService : ITextCompletion
{
    private readonly OpenAIClient _openAIClient;
    private readonly string _deployedModelName;

    public AzureOpenAIChatCompletionService(OpenAIClient openAIClient, IOptions<OpenAIConfig> options)
    {
        _openAIClient = openAIClient;
        _deployedModelName = options.Value.ChatGptDeployment;
    }

    public async Task<IReadOnlyList<ITextCompletionResult>> GetCompletionsAsync(string text, CompleteRequestSettings requestSettings, CancellationToken cancellationToken = default)
    {
        var option = new CompletionsOptions
        {
            MaxTokens = requestSettings.MaxTokens,
            FrequencyPenalty = Convert.ToSingle(requestSettings.FrequencyPenalty),
            PresencePenalty = Convert.ToSingle(requestSettings.PresencePenalty),
            Temperature = Convert.ToSingle(requestSettings.Temperature),
            Prompts = { text },
        };

        foreach (var stopSequence in requestSettings.StopSequences)
        {
            option.StopSequences.Add(stopSequence);
        }

        var response =
            await _openAIClient.GetCompletionsAsync(
                _deployedModelName, option, cancellationToken);
        if (response.Value is Completions completions && completions.Choices.Count > 0)
        {
            return response.Value.Choices.Select(c => new TextCompletionResult(c.Text)).ToList();
        }
        else
        {
            throw new AIException(AIException.ErrorCodes.InvalidConfiguration, "completion not found");
        }
    }

    public IAsyncEnumerable<ITextCompletionStreamingResult> GetStreamingCompletionsAsync(string text, CompleteRequestSettings requestSettings, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}


public sealed class TextCompletionResult : ITextCompletionResult
{
    private readonly string result;
    public TextCompletionResult(string result)
    {
        this.result = result;
    }

    public async Task<string> GetCompletionAsync(CancellationToken cancellationToken = default)
    {
        return await Task.FromResult(result);
    }
}
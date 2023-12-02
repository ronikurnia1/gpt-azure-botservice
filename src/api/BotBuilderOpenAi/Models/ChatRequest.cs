namespace BotBuilderOpenAi.Models;

public record class ChatRequest(ChatTurn[] History, RequestOverrides? Overrides = null)
{
    public string? LastUserQuestion => History?.LastOrDefault()?.User;
}

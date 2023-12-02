namespace BotBuilderOpenAi.Models;

public class ConversationData
{
    public string ConversationId { get; set; } = string.Empty;
    public List<ChatTurnData> ChatTurns { get; set; } = [];
}

public class ChatTurnData
{
    public string User { get; set; } = string.Empty;
    public string Bot { get; set; } = string.Empty;
}

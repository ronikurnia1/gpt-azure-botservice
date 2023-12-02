namespace BotBuilderOpenAi.Models;

public record SupportingContentRecord(string Title, string Content);

public record Response(
    string Answer,
    string? Thoughts,
    SupportingContentRecord[] DataPoints,
    string CitationBaseUrl,
    string? Error = null);

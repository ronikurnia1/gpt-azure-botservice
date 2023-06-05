using Microsoft.Bot.Schema;
using System.Text.RegularExpressions;

namespace GptBotApi.Extensions;

internal static partial class ResponseExtensions
{

    internal static List<Attachment> ToAttachments(this List<CitationDetails> citations)
    {
        return citations.Select(c => new Attachment
        {
            ContentType = "application/octet-stream",
            ContentUrl = $"{c.BaseUrl}/{c.Name}",
            Name = $"[{c.Number}] {c.Name}",
            Content = $"[{c.Number}] {c.Name}"
        }).ToList();
    }

    internal static Attachment? ToAttachment(this HashSet<string> followUps)
    {
        if (followUps == null || followUps.Count == 0) return null;
        return new HeroCard
        {
            Text = "Follow up questions:",
            Buttons = followUps.Select(c => new CardAction()
            {
                Type = ActionTypes.ImBack,
                Title = c,
                Value = c
            }).ToList()
        }.ToAttachment();
    }

    internal static ParsedResponse ParseResponse(this string answer, string citationBaseUrl)
    {
        var citations = new List<CitationDetails>();
        var followUpQuestions = new HashSet<string>();

        var parsedAnswer = ReplacementRegex().Replace(answer, match =>
        {
            followUpQuestions.Add(match.Value);
            return "";
        });

        parsedAnswer = parsedAnswer.Trim();

        var parts = SplitRegex().Split(parsedAnswer);

        var fragments = parts.Select((fileName, index) =>
        {
            if (index % 2 is 0)
            {
                return fileName;
            }
            else
            {
                var citationNumber = citations.Count + 1;
                var existingCitation = citations.FirstOrDefault(c => c.Name == fileName);
                if (existingCitation is not null)
                {
                    citationNumber = existingCitation.Number;
                    return $"""[{citationNumber}]""";
                }
                else
                {
                    if (fileName.Length > 4)
                    {
                        // Min file name length is 5
                        var citation = new CitationDetails(fileName, citationBaseUrl, citationNumber);
                        citations.Add(citation);
                        return $"""[{citationNumber}]""";
                    }
                }
                return "";
            }
        });

        return new ParsedResponse(
            string.Join("", fragments),
            citations,
            followUpQuestions.Select(f => f.Replace("<<", "").Replace(">>", ""))
                .ToHashSet());
    }

    [GeneratedRegex(@"<<([^>>]+)>>", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex ReplacementRegex();

    [GeneratedRegex(@"\[([^\]]+)\]", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex SplitRegex();
}

internal readonly record struct ParsedResponse(
    string Answer,
    List<CitationDetails> Citations,
    HashSet<string> FollowUpQuestions);


public record CitationDetails(string Name, string BaseUrl, int Number = 0);
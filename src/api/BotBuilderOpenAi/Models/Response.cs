// Copyright (c) Microsoft. All rights reserved.

namespace BotBuilderOpenAi.Models;

public record Response(
    string Answer,
    string? Thoughts,
    string[] DataPoints,
    string CitationBaseUrl,
    string? Error = null);

// Copyright (c) Microsoft. All rights reserved.

namespace BotBuilderOpenAi.Models;

public record ChatTurn(string User, string? Bot = null);

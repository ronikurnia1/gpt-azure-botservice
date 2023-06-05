﻿// Copyright (c) Microsoft. All rights reserved.

using Azure.Search.Documents;
using BotBuilderOpenAi.Extensions;
using BotBuilderOpenAi.Models;
using Microsoft.SemanticKernel.AI;
using Microsoft.SemanticKernel.Orchestration;
using Microsoft.SemanticKernel.SkillDefinition;

namespace BotBuilderOpenAi.Skills;

public sealed class LookupSkill
{
    private readonly SearchClient _searchClient;
    private readonly RequestOverrides? _requestOverrides;

    public LookupSkill(SearchClient searchClient, RequestOverrides? requestOverrides)
    {
        _searchClient = searchClient;
        _requestOverrides = requestOverrides;
    }

    [SKFunction("Look up knowledge")]
    [SKFunctionName("Lookup")]
    [SKFunctionInput(Description = "lookup query")]
    public async Task<string> ExecAsync(string lookupQuery, SKContext context)
    {
        if (lookupQuery is string query)
        {
            return await _searchClient.LookupAsync(query, _requestOverrides);
        }

        throw new AIException(
            AIException.ErrorCodes.ServiceError,
            "Query skill failed to get query from context");
    }
}

using System.Text.Json.Nodes;
using Anthropic.SDK;
using Anthropic.SDK.Messaging;
using Tool = Anthropic.SDK.Common.Tool;
using Function = Anthropic.SDK.Common.Function;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Options;
using Logistics.Infrastructure.AI.Prompts;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// Builds the Claude API conversation: client, system prompt, tools, and initial message.
/// </summary>
internal sealed class DispatchConversationBuilder(
    IDispatchToolRegistry toolRegistry,
    ITenantUnitOfWork tenantUow,
    ILogger<DispatchConversationBuilder> logger)
{
    public (AnthropicClient Client, MessageParameters Parameters, List<Message> Messages)
        Build(DispatchSession session, DispatchAgentMode mode, ClaudeOptions config)
    {
        if (string.IsNullOrWhiteSpace(config.ApiKey))
            throw new InvalidOperationException("Claude API key is not configured.");

        var tenant = tenantUow.GetCurrentTenant();
        var companyName = tenant.Name ?? "Fleet";
        var systemPrompt = DispatchSystemPrompt.Build(companyName, mode);

        var tools = toolRegistry.GetToolDefinitions()
            .Select<DispatchToolDefinition, Tool>(t => new Function(t.Name, t.Description, (JsonNode)t.InputSchema))
            .ToList();

        logger.LogInformation(
            "Agent session {SessionId} initialized with {ToolCount} tools, model {Model}",
            session.Id, tools.Count, config.Model);

        var modeLabel = mode == DispatchAgentMode.Autonomous ? "autonomous" : "suggestions";
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC");

        var messages = new List<Message>
        {
            new(RoleType.User,
                $"Analyze the current fleet state and optimize dispatch assignments. " +
                $"Current time: {timestamp}. Mode: {modeLabel}. " +
                $"Start by calling get_unassigned_loads and get_available_trucks together, then process all loads efficiently.")
        };

        var client = new AnthropicClient(config.ApiKey);

        var parameters = new MessageParameters
        {
            Messages = messages,
            MaxTokens = config.MaxTokens,
            Model = config.Model,
            Stream = false,
            Temperature = 0m,
            System = [new SystemMessage(systemPrompt)],
            Tools = tools
        };

        return (client, parameters, messages);
    }
}

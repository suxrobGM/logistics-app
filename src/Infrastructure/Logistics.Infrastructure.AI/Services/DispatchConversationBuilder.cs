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
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// Builds the Claude API conversation: client, system prompt, tools, and initial message.
/// </summary>
internal sealed class DispatchConversationBuilder(
    IDispatchToolRegistry toolRegistry,
    IFeatureService featureService,
    ITenantUnitOfWork tenantUow,
    ILogger<DispatchConversationBuilder> logger)
{
    public async Task<(AnthropicClient Client, MessageParameters Parameters, List<Message> Messages)>
        BuildAsync(DispatchSession session, DispatchAgentRequest request, LlmOptions config)
    {
        if (string.IsNullOrWhiteSpace(config.ApiKey))
            throw new InvalidOperationException("Claude API key is not configured.");

        var tenant = tenantUow.GetCurrentTenant();
        var companyName = tenant.Name ?? "Fleet";

        // Check load board feature for this tenant
        var hasLoadBoard = await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.LoadBoard);
        var systemPrompt = DispatchSystemPrompt.Build(companyName, request.Mode, hasLoadBoard);

        var tools = toolRegistry.GetToolDefinitions(includeLoadBoardTools: hasLoadBoard)
            .Select<DispatchToolDefinition, Tool>(t => new Function(t.Name, t.Description, (JsonNode)t.InputSchema))
            .ToList();

        // Resolve model: per-tenant override → system default
        var model = tenant.Settings.LlmModel ?? config.Model;
        if (!LlmOptions.AllowedModels.Contains(model))
            model = config.Model;

        session.ModelUsed = model;

        logger.LogInformation(
            "Agent session {SessionId} initialized with {ToolCount} tools, model {Model}",
            session.Id, tools.Count, model);

        // Build user message with optional context
        var userMessage = BuildUserMessage(request);

        // Inject previous session context for conversation memory
        var previousContext = await GetPreviousSessionContextAsync();
        if (previousContext is not null)
            userMessage = $"{previousContext}\n\n{userMessage}";

        var messages = new List<Message>
        {
            new(RoleType.User, userMessage)
        };

        var client = new AnthropicClient(config.ApiKey);

        var parameters = new MessageParameters
        {
            Messages = messages,
            MaxTokens = config.MaxTokens,
            Model = model,
            Stream = false,
            Temperature = 0m,
            System = [new SystemMessage(systemPrompt) { CacheControl = new CacheControl { Type = CacheControlType.ephemeral } }],
            Tools = tools
        };

        // Extended thinking: mutually exclusive with Temperature
        var enableThinking = tenant.Settings.LlmExtendedThinking ?? config.EnableExtendedThinking;
        if (enableThinking)
        {
            parameters.Temperature = null;
            parameters.Thinking = new ThinkingParameters
            {
                Type = ThinkingType.enabled,
                BudgetTokens = config.ThinkingBudgetTokens
            };
            parameters.MaxTokens = Math.Max(config.MaxTokens, config.ThinkingBudgetTokens + 4096);
        }

        return (client, parameters, messages);
    }

    private static string BuildUserMessage(DispatchAgentRequest request)
    {
        var modeLabel = request.Mode == DispatchAgentMode.Autonomous ? "autonomous" : "suggestions";
        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC");

        var message = $"Analyze the current fleet state and optimize dispatch assignments. " +
            $"Current time: {timestamp}. Mode: {modeLabel}. " +
            $"Start by calling get_unassigned_loads and get_available_trucks together, then process all loads efficiently.";

        if (!string.IsNullOrWhiteSpace(request.Instructions))
        {
            var sanitized = SanitizeInstructions(request.Instructions);
            message += $"\n\nDispatcher instructions: {sanitized}";
        }

        if (!string.IsNullOrWhiteSpace(request.RejectionContext))
        {
            var sanitized = SanitizeInstructions(request.RejectionContext);
            message += $"\n\nContext from rejected decisions: {sanitized}";
        }

        return message;
    }

    private async Task<string?> GetPreviousSessionContextAsync()
    {
        var lastSession = await tenantUow.Repository<DispatchSession>().Query()
            .Where(s => s.Status == DispatchSessionStatus.Completed && s.Summary != null)
            .OrderByDescending(s => s.CompletedAt)
            .Select(s => new { s.Number, s.CompletedAt, s.Summary })
            .FirstOrDefaultAsync();

        if (lastSession is null)
            return null;

        var summary = lastSession.Summary!.Length > 1000
            ? lastSession.Summary[..1000]
            : lastSession.Summary;

        return $"Context from previous session (#{lastSession.Number}, {lastSession.CompletedAt:yyyy-MM-dd HH:mm UTC}): {summary}";
    }

    private static string SanitizeInstructions(string input)
    {
        // Strip control characters to prevent prompt injection
        var sanitized = new string([.. input.Where(c => !char.IsControl(c) || c == '\n')]);
        return sanitized.Length > 500 ? sanitized[..500] : sanitized;
    }
}

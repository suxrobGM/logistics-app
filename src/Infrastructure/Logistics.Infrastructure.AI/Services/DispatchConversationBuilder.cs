using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Options;
using Logistics.Infrastructure.AI.Prompts;
using Logistics.Infrastructure.AI.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// Builds the LLM conversation: provider, system prompt, tools, and initial message.
/// Provider-agnostic - delegates SDK-specific work to <see cref="ILlmProvider"/>.
/// </summary>
internal sealed class DispatchConversationBuilder(
    IDispatchToolRegistry toolRegistry,
    IFeatureService featureService,
    LlmProviderFactory providerFactory,
    ITenantUnitOfWork tenantUow,
    ILogger<DispatchConversationBuilder> logger)
{
    public async Task<LlmConversation> BuildAsync(
        DispatchSession session,
        DispatchAgentRequest request,
        LlmOptions config)
    {
        var tenant = tenantUow.GetCurrentTenant();
        var companyName = tenant.Name ?? "Fleet";

        // Resolve provider: tenant preference → system default
        var resolvedProvider = tenant.Settings.LlmProvider ?? config.DefaultProvider;
        var provider = providerFactory.Create(resolvedProvider);
        var providerConfig = config.GetProviderConfig(resolvedProvider);

        if (string.IsNullOrWhiteSpace(providerConfig.ApiKey))
            throw new InvalidOperationException("LLM API key is not configured.");

        // Check load board feature for this tenant
        var hasLoadBoard = await featureService.IsFeatureEnabledAsync(tenant.Id, TenantFeature.LoadBoard);
        var systemPrompt = DispatchSystemPrompt.Build(companyName, request.Mode, hasLoadBoard, tenant.Settings.DistanceUnit);
        var tools = toolRegistry.GetToolDefinitions(includeLoadBoardTools: hasLoadBoard);

        // Resolve model: tenant selection → provider default
        var model = tenant.Settings.LlmModel ?? providerConfig.Model;
        session.ModelUsed = model;

        logger.LogInformation(
            "Agent session {SessionId} initialized with {ToolCount} tools, model {Model}, provider {Provider}",
            session.Id, tools.Count, model, resolvedProvider);

        // Build user message with optional context
        var userMessage = BuildUserMessage(request);
        var previousContext = await GetPreviousSessionContextAsync();
        if (previousContext is not null)
            userMessage = $"{previousContext}\n\n{userMessage}";

        var messages = new List<LlmMessage> { LlmMessage.FromUser(userMessage) };

        // Build thinking options
        LlmThinkingOptions? thinking = null;
        var enableThinking = tenant.Settings.LlmExtendedThinking ?? config.EnableExtendedThinking;
        if (enableThinking)
            thinking = new LlmThinkingOptions(config.ThinkingBudgetTokens);

        return new LlmConversation(provider, systemPrompt, messages, tools, model, config.MaxTokens, thinking);
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

/// <summary>
/// Provider-agnostic conversation state returned by <see cref="DispatchConversationBuilder"/>.
/// </summary>
internal record LlmConversation(
    ILlmProvider Provider,
    string SystemPrompt,
    List<LlmMessage> Messages,
    IReadOnlyList<DispatchToolDefinition> Tools,
    string Model,
    int MaxTokens,
    LlmThinkingOptions? Thinking);

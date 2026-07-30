using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Application.Abstractions.AI;
using Logistics.Infrastructure.AI.Prompts;
using Logistics.Infrastructure.AI.Llm;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.SystemSettings;

namespace Logistics.Infrastructure.AI.Services;

/// <summary>
/// Builds the LLM conversation: provider, system prompt, tools, and initial message.
/// Provider-agnostic - delegates SDK-specific work to <see cref="ILlmProvider"/>.
/// </summary>
internal sealed class AIDispatchConversationBuilder(
    IAIDispatchToolRegistry toolRegistry,
    LlmSessionSetup sessionSetup,
    ITenantUnitOfWork tenantUow,
    ISystemSettingsService systemSettings,
    ILogger<AIDispatchConversationBuilder> logger)
{
    public async Task<LlmConversation> BuildAsync(
        AIDispatchSession session,
        AIDispatchRequest request,
        LlmOptions config,
        CancellationToken ct)
    {
        var setup = await sessionSetup.ResolveAsync(config, ct);
        var tenant = setup.Tenant;
        var companyName = tenant.Name ?? "Fleet";
        var resolvedProvider = setup.Selection.Provider;

        var enabledFeatures = setup.EnabledFeatures;
        var hasLoadBoard = enabledFeatures.Contains(TenantFeature.LoadBoard);
        var hasIntermodal = enabledFeatures.Contains(TenantFeature.IntermodalContainers);

        var policy = await GetLearnedPolicyAsync(ct);
        var systemPrompt = AIDispatchSystemPrompt.Build(
            companyName, request.Mode, hasLoadBoard, tenant.Settings.DistanceUnit, policy, hasIntermodal,
            tenant.Settings.OperatingMode);
        // No caller permissions: a dispatch run is gated by the endpoint's policy, not per tool.
        var tools = toolRegistry.GetToolDefinitions(enabledFeatures, forDispatchAgent: true);

        var model = setup.Selection.Model;
        session.ModelUsed = model;

        logger.LogInformation(
            "Agent session {SessionId} initialized with {ToolCount} tools, model {Model}, provider {Provider}",
            session.Id, tools.Count, model, resolvedProvider);

        var userMessage = BuildUserMessage(request);
        var previousContext = await GetPreviousSessionContextAsync(ct);
        if (previousContext is not null)
            userMessage = $"{previousContext}\n\n{userMessage}";

        var messages = new List<LlmMessage> { LlmMessage.FromUser(userMessage) };

        // Build thinking options: global system setting → appsettings default.
        // Only honored by providers/models that support it; others ignore it.
        LlmThinkingOptions? thinking = null;
        var thinkingSetting = await systemSettings.GetAsync(AISettingsKeys.ExtendedThinking, ct);
        var enableThinking = bool.TryParse(thinkingSetting, out var parsedThinking)
            ? parsedThinking
            : config.EnableExtendedThinking;
        if (enableThinking)
            thinking = new LlmThinkingOptions(config.ThinkingBudgetTokens);

        return new LlmConversation(setup.Provider, systemPrompt, messages, tools, model, config.MaxTokens, thinking);
    }

    private static string BuildUserMessage(AIDispatchRequest request)
    {
        var modeLabel = request.Mode == AIDispatchMode.Autonomous ? "autonomous" : "suggestions";
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

    /// <summary>
    /// Loads the tenant's learned dispatch policy. Filtering <c>IsEnabled</c> in SQL means a disabled
    /// policy is simply absent, so the prompt section omits itself.
    /// </summary>
    private async Task<LearnedDispatchPolicy?> GetLearnedPolicyAsync(CancellationToken ct)
    {
        return await tenantUow.Repository<AIDispatchPolicy>().Query()
            .Where(p => p.IsEnabled)
            .Select(p => new LearnedDispatchPolicy(p.ManualContent, p.GeneratedContent))
            .FirstOrDefaultAsync(ct);
    }

    private async Task<string?> GetPreviousSessionContextAsync(CancellationToken ct)
    {
        var lastSession = await tenantUow.Repository<AIDispatchSession>().Query()
            .DispatchOnly()
            .Where(s => s.Status == AIDispatchSessionStatus.Completed && s.Summary != null)
            .OrderByDescending(s => s.CompletedAt)
            .Select(s => new { s.Number, s.CompletedAt, s.Summary })
            .FirstOrDefaultAsync(ct);

        if (lastSession is null)
            return null;

        var summary = lastSession.Summary!.Length > 1000
            ? lastSession.Summary[..1000]
            : lastSession.Summary;

        return $"Context from previous session (#{lastSession.Number}, {lastSession.CompletedAt:yyyy-MM-dd HH:mm UTC}): {summary}";
    }

    private static string SanitizeInstructions(string input)
    {
        var sanitized = PromptText.StripControlChars(input, allowLineBreaks: true);
        return sanitized.Length > 500 ? sanitized[..500] : sanitized;
    }
}

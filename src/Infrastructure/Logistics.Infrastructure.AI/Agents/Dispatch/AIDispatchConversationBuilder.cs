using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Application.Abstractions.AI;
using Logistics.Infrastructure.AI.Llm;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Infrastructure.AI.Agents.Dispatch;

/// <summary>
/// Builds the LLM conversation for one dispatch turn: system prompt (with learned policy), dispatch
/// tool catalogue, and the replayed transcript with a turn-context note on the final user message.
/// </summary>
internal sealed class AIDispatchConversationBuilder(
    IAgentToolRegistry toolRegistry,
    LlmSessionSetup sessionSetup,
    ITenantUnitOfWork tenantUow,
    ILogger<AIDispatchConversationBuilder> logger)
{
    public async Task<LlmConversation> BuildAsync(
        AgentSession session,
        AgentConversation conversation,
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
        var systemPrompt = AIDispatchSystemPrompt.Build(new(companyName)
        {
            DistanceUnit = tenant.Settings.DistanceUnit,
            OperatingMode = tenant.Settings.OperatingMode,
            HasLoadBoardIntegration = hasLoadBoard,
            HasIntermodal = hasIntermodal,
            Policy = policy
        });
        // No caller permissions: a dispatch turn is gated by the endpoint's policy, not per tool.
        var tools = toolRegistry.GetDispatchAgentTools(enabledFeatures);

        var model = setup.Selection.Model;
        session.ModelUsed = model;

        logger.LogInformation(
            "Dispatch turn {SessionId} initialized with {ToolCount} tools, model {Model}, provider {Provider}",
            session.Id, tools.Count, model, resolvedProvider);

        var messages = AgentTranscriptReplay.BuildMessages(conversation.Messages);
        InjectTurnContext(messages);

        return new LlmConversation(
            setup.Provider, systemPrompt, messages, tools, model, config.MaxTokens, setup.Effort);
    }

    /// <summary>
    /// Stamps the current time and makes the agent re-gather fleet state before acting, without
    /// forcing tool calls on conversational messages. In-memory only - never persisted to the row's
    /// <c>ContentJson</c>, so a stale notice cannot replay later.
    /// </summary>
    private static void InjectTurnContext(List<LlmMessage> messages)
    {
        if (messages.Count == 0 || messages[^1].Role != LlmRole.User)
            return;

        var timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm UTC");
        messages[^1].Content.Add(new LlmTextBlock(
            $"[Current time: {timestamp}] This timestamp is authoritative for any date or time reasoning. " +
            "Fleet data gathered in earlier turns may be stale: if you are about to propose or take any " +
            "dispatch action, call get_unassigned_loads and get_available_trucks together first. If this " +
            "message needs no dispatch action, just answer it - no tool calls."));
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
}

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
/// tool catalogue, and the replayed transcript.
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
        var hasRateNegotiation = hasLoadBoard && enabledFeatures.Contains(TenantFeature.AIRateNegotiation);

        var policy = await GetLearnedPolicyAsync(ct);
        var systemPrompt = AIDispatchSystemPrompt.Build(new(companyName)
        {
            DistanceUnit = tenant.Settings.DistanceUnit,
            IsSolo = tenant.IsSolo,
            HasLoadBoardIntegration = hasLoadBoard,
            HasIntermodal = hasIntermodal,
            HasVehicleTransport = enabledFeatures.Contains(TenantFeature.VehicleTransport),
            HasRateNegotiation = hasRateNegotiation,
            Policy = policy
        });
        // No caller permissions: a dispatch turn is gated by the endpoint's policy, not per tool.
        var tools = toolRegistry.GetDispatchAgentTools(enabledFeatures);

        var model = setup.Selection.Model;
        session.ModelUsed = model;

        logger.LogInformation(
            "Dispatch turn {SessionId} initialized with {ToolCount} tools, model {Model}, provider {Provider}",
            session.Id, tools.Count, model, resolvedProvider);

        var messages = await AgentTranscriptReplay.LoadAsync(tenantUow, conversation, ct);

        return new LlmConversation(
            setup.Provider, systemPrompt, messages, tools, model, config.MaxTokens, setup.Effort);
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

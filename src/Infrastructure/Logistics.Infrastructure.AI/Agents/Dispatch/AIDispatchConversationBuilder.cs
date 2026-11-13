using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.Context;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Agents.Dispatch;

internal sealed class AIDispatchConversationBuilder(
    IAgentToolRegistry toolRegistry,
    LlmSessionSetup sessionSetup,
    ITenantUnitOfWork tenantUow,
    AgentTranscriptLoader transcriptLoader,
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
        var hasRateNegotiation = hasLoadBoard && enabledFeatures.Contains(TenantFeature.AINegotiation);

        var policy = await GetLearnedPolicyAsync(ct);
        var systemPrompt = AIDispatchSystemPrompt.Build(new(companyName)
        {
            DistanceUnit = tenant.Settings.DistanceUnit,
            Currency = tenant.Currency,
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

        var messages = await transcriptLoader.LoadAsync(conversation, ct);

        return new LlmConversation(
            setup.Provider, systemPrompt, messages, tools, model, config.MaxTokens, setup.Effort);
    }

    /// <summary>The tenant's enabled learned dispatch policy, or null when there is none; null leaves the policy section out of the prompt.</summary>
    private async Task<LearnedDispatchPolicy?> GetLearnedPolicyAsync(CancellationToken ct)
    {
        return await tenantUow.Repository<AIDispatchPolicy>().Query()
            .Where(p => p.IsEnabled)
            .Select(p => new LearnedDispatchPolicy(p.ManualContent, p.GeneratedContent))
            .FirstOrDefaultAsync(ct);
    }
}

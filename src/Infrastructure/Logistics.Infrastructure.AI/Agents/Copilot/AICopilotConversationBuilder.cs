using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.AI.Llm.Contracts;
using Logistics.Application.Abstractions.AI;
using Logistics.Application.Abstractions.Features;
using Logistics.Infrastructure.AI.Llm;
using Logistics.Shared.Identity.Policies;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.AI.Agents.Copilot;

/// <summary>
/// Builds the LLM conversation for one copilot turn: system prompt, the caller-scoped tool
/// catalogue, and the message sequence rebuilt from the persisted transcript.
/// </summary>
internal sealed class AICopilotConversationBuilder(
    IAgentToolRegistry toolRegistry,
    LlmSessionSetup sessionSetup,
    ITenantUnitOfWork tenantUow,
    ILogger<AICopilotConversationBuilder> logger)
{
    public async Task<LlmConversation> BuildAsync(
        AgentSession session,
        AgentConversation conversation,
        IReadOnlySet<string> callerPermissions,
        LlmOptions config,
        CancellationToken ct)
    {
        var setup = await sessionSetup.ResolveAsync(config, ct);
        var tenant = setup.Tenant;
        var selection = setup.Selection;

        var tools = toolRegistry.GetCopilotTools(setup.EnabledFeatures, callerPermissions);

        var systemPrompt = AICopilotSystemPrompt.Build(new(tenant.Name ?? "Fleet")
        {
            DistanceUnit = tenant.Settings.DistanceUnit,
            IsSolo = tenant.IsSolo,
            // Read from metadata, so a new dispatch write tool brings the guardrails along.
            HasDispatchTools =
                tools.Any(t => t.IsWrite && t.RequiredPermission == Permission.Dispatch.Manage)
        });

        session.ModelUsed = selection.Model;

        logger.LogInformation(
            "Copilot turn {SessionId} initialized with {ToolCount} tools, model {Model}, provider {Provider}",
            session.Id, tools.Count, selection.Model, selection.Provider);

        var messages = await AgentTranscriptReplay.LoadAsync(tenantUow, conversation, ct);

        // Thinking blocks replay only within the turn; the persisted transcript drops them.
        return new LlmConversation(
            setup.Provider, systemPrompt, messages, tools, selection.Model, config.MaxTokens, setup.Effort);
    }
}

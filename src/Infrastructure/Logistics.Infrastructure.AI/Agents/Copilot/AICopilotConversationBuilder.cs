using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
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
            OperatingMode = tenant.Settings.OperatingMode,
            // Metadata, not a name list: a new dispatch write tool must not silently lose the
            // guardrails section just because nobody remembered to extend a hardcoded pair.
            HasDispatchTools =
                tools.Any(t => t.IsWrite && t.RequiredPermission == Permission.Dispatch.Manage)
        });

        session.ModelUsed = selection.Model;

        logger.LogInformation(
            "Copilot turn {SessionId} initialized with {ToolCount} tools, model {Model}, provider {Provider}",
            session.Id, tools.Count, selection.Model, selection.Provider);

        var messages = AgentTranscriptReplay.BuildMessages(conversation.Messages);

        // Same admin-set effort as dispatch. Thinking blocks replay in-turn; the persisted
        // transcript drops them, which is fine - prior turns replay without them.
        return new LlmConversation(
            setup.Provider, systemPrompt, messages, tools, selection.Model, config.MaxTokens, setup.Effort);
    }
}

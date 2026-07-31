using Logistics.Application.Abstractions.Agents;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
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
    private const int MaxTranscriptMessages = 30;

    public async Task<LlmConversation> BuildAsync(
        AgentSession session,
        AICopilotConversation conversation,
        IReadOnlySet<string> callerPermissions,
        LlmOptions config,
        string? pageContext,
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
                tools.Any(t => t.IsWrite && t.RequiredPermission == Permission.Dispatch.Manage),
            PageContext = pageContext
        });

        session.ModelUsed = selection.Model;

        logger.LogInformation(
            "Copilot turn {SessionId} initialized with {ToolCount} tools, model {Model}, provider {Provider}",
            session.Id, tools.Count, selection.Model, selection.Provider);

        var messages = BuildMessages(conversation.Messages);

        // Thinking stays off: thinking blocks are not persisted, and replaying prior assistant
        // turns without them violates provider requirements.
        return new LlmConversation(
            setup.Provider, systemPrompt, messages, tools, selection.Model, config.MaxTokens, Thinking: null);
    }

    private static List<LlmMessage> BuildMessages(IEnumerable<AICopilotMessage> transcript)
    {
        var ordered = transcript.OrderBy(m => m.Sequence).ToList();
        var (window, truncated) = TakeRecentWindow(ordered);

        var messages = new List<LlmMessage>();
        foreach (var row in window)
        {
            var blocks = row.Role == AICopilotMessageRole.System
                ? [new LlmTextBlock($"[system note] {row.DisplayText}")]
                : CopilotTranscriptCodec.Decode(row.ContentJson);

            if (blocks.Count == 0)
                continue;

            // System rows replay as user-role notes - providers only accept user/assistant mid-conversation.
            var role = row.Role == AICopilotMessageRole.Assistant ? LlmRole.Assistant : LlmRole.User;
            messages.Add(new LlmMessage(role, blocks));
        }

        if (truncated && messages.Count > 0 && messages[0].Role == LlmRole.User)
            messages[0].Content.Insert(0, new LlmTextBlock("[Earlier conversation omitted.]"));

        return messages;
    }

    /// <summary>
    /// Cuts only at a plain user chat message: starting the window mid-turn orphans a
    /// tool_use/tool_result pair and the provider rejects the request.
    /// </summary>
    private static (List<AICopilotMessage> Window, bool Truncated) TakeRecentWindow(
        List<AICopilotMessage> ordered)
    {
        if (ordered.Count <= MaxTranscriptMessages)
            return (ordered, false);

        var start = ordered.Count - MaxTranscriptMessages;
        while (start < ordered.Count && !IsChatBoundary(ordered[start]))
            start++;

        // No boundary in the window - replay everything rather than corrupt it.
        return start >= ordered.Count ? (ordered, false) : (ordered[start..], start > 0);
    }

    private static bool IsChatBoundary(AICopilotMessage row) =>
        row.Role != AICopilotMessageRole.Assistant && row.DisplayText is not null;
}

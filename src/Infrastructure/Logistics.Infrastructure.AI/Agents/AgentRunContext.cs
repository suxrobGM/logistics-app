using Logistics.Application.Abstractions.Agents;

namespace Logistics.Infrastructure.AI.Agents;

/// <inheritdoc />
internal sealed class AgentRunContext : IAgentRunContext
{
    public Guid? TriggeredByUserId { get; private set; }
    public Guid? ConversationId { get; private set; }
    public Guid? DecisionId { get; private set; }

    public void SetTriggeredBy(Guid? userId) => TriggeredByUserId = userId;

    public void SetConversation(Guid? conversationId) => ConversationId = conversationId;

    public void SetDecision(Guid? decisionId) => DecisionId = decisionId;
}

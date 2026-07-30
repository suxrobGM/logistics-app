using Logistics.Application.Abstractions.Agents;

namespace Logistics.Infrastructure.AI.Agents;

/// <inheritdoc />
internal sealed class AgentRunContext : IAgentRunContext
{
    public Guid? TriggeredByUserId { get; private set; }

    public void SetTriggeredBy(Guid? userId) => TriggeredByUserId = userId;
}

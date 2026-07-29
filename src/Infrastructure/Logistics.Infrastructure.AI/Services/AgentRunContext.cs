using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.Infrastructure.AI.Services;

/// <inheritdoc />
internal sealed class AgentRunContext : IAgentRunContext
{
    public Guid? TriggeredByUserId { get; private set; }

    public void SetTriggeredBy(Guid? userId) => TriggeredByUserId = userId;
}

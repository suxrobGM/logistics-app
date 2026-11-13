using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.AI.Agents.Services;

internal interface IAgentDecisionApprover : IApplicationService
{
    /// <summary>Checks the tool's own permission, since the surface's Manage alone is too broad.</summary>
    Task<Result> ApproveAsync(
        AgentDecision decision,
        AgentConversation conversation,
        Guid userId,
        Guid tenantId,
        Func<AgentMessageDto, Task> broadcastMessageAsync,
        CancellationToken ct);
}

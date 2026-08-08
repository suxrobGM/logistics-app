using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal interface IAgentDecisionAuthorization : IApplicationService
{
    /// <summary>
    /// A surface's own Manage permission is not enough to execute, say, an invoice write: the tool
    /// carries its own required permission and the approver must hold it. Shared so a tool gated on
    /// one surface cannot slip through unguarded on the other.
    /// </summary>
    Task<Result> EnsureToolPermissionAsync(
        AgentDecision decision, Guid userId, Guid tenantId, CancellationToken ct);
}

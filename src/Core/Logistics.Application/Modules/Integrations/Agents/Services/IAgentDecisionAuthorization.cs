using Logistics.Application.Abstractions.Common;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal interface IAgentDecisionAuthorization : IApplicationService
{
    /// <summary>
    /// A surface's own Manage permission is not enough to execute, say, an invoice write - the
    /// approver must also hold the tool's own required permission.
    /// </summary>
    Task<Result> EnsureToolPermissionAsync(
        AgentDecision decision, Guid userId, Guid tenantId, CancellationToken ct);
}

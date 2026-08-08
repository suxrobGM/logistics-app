using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal sealed class AgentDecisionAuthorization(
    IAgentToolRegistry toolRegistry,
    IUserPermissionService userPermissions) : IAgentDecisionAuthorization
{
    public async Task<Result> EnsureToolPermissionAsync(
        AgentDecision decision, Guid userId, Guid tenantId, CancellationToken ct)
    {
        if (toolRegistry.TryGetDefinition(decision.ToolName!)?.RequiredPermission is not { } required)
            return Result.Ok();

        return await userPermissions.HasPermissionAsync(userId, tenantId, required, ct)
            ? Result.Ok()
            : Result.Fail($"You need the {required} permission to approve this action");
    }
}

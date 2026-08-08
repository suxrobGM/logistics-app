using Logistics.Application.Abstractions.Agents;
using Logistics.Application.Modules.IdentityAccess.Users.Queries;
using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

internal sealed class AgentDecisionAuthorization(
    IAgentToolRegistry toolRegistry,
    IMediator mediator) : IAgentDecisionAuthorization
{
    public async Task<Result> EnsureToolPermissionAsync(
        AgentDecision decision, Guid userId, Guid tenantId, CancellationToken ct)
    {
        if (toolRegistry.TryGetDefinition(decision.ToolName!)?.RequiredPermission is not { } required)
            return Result.Ok();

        var permissions = await mediator.Send(new GetCurrentUserPermissionsQuery
        {
            UserId = userId,
            TenantId = tenantId
        }, ct);

        return permissions.Value?.Contains(required) == true
            ? Result.Ok()
            : Result.Fail($"You need the {required} permission to approve this action");
    }
}

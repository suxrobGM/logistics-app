using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Modules.Integrations.Agents;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

/// <summary>
/// CreatedById is audit-only here - dispatch conversations are tenant-shared, so every user with
/// Dispatch.View can read this one and any with Dispatch.Manage can act on it.
/// </summary>
internal sealed class CreateAIDispatchConversationHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser)
    : IAppRequestHandler<CreateAIDispatchConversationCommand, Result<AgentConversationDto>>
{
    public Task<Result<AgentConversationDto>> Handle(
        CreateAIDispatchConversationCommand request, CancellationToken ct) =>
        AgentConversationCommands.CreateAsync(
            tenantUow, AgentConversationKind.Dispatch, currentUser.GetUserId(), ct);
}

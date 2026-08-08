using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Commands;

internal sealed class CreateAIDispatchConversationHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser) : IAppRequestHandler<CreateAIDispatchConversationCommand, Result<AgentConversationDto>>
{
    public async Task<Result<AgentConversationDto>> Handle(
        CreateAIDispatchConversationCommand request, CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        if (userId is null)
            return Result<AgentConversationDto>.Fail("User is not authenticated");

        // CreatedById is audit-only here - dispatch conversations are tenant-shared, so every user
        // with Dispatch.View can read this one and any with Dispatch.Manage can act on it.
        var conversation = new AgentConversation
        {
            CreatedById = userId.Value,
            Kind = AgentConversationKind.Dispatch
        };
        await tenantUow.Repository<AgentConversation>().AddAsync(conversation, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<AgentConversationDto>.Ok(conversation.ToDto());
    }
}

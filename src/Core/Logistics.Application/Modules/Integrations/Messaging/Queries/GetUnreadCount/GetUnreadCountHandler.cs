using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Messaging.Queries;

internal sealed class GetUnreadCountHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetUnreadCountQuery, Result<int>>
{
    public async Task<Result<int>> Handle(GetUnreadCountQuery req, CancellationToken ct)
    {
        var totalUnread = await tenantUow.Repository<Message>().CountAsync(
            m => m.SenderId != req.EmployeeId
                 && m.Conversation.Participants.Any(p => p.EmployeeId == req.EmployeeId
                                                        && (p.LastReadAt == null || m.SentAt > p.LastReadAt)),
            ct);

        return Result<int>.Ok(totalUnread);
    }
}

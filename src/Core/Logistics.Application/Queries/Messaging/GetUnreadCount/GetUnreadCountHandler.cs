using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Messaging;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetUnreadCountHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetUnreadCountQuery, Result<int>>
{
    public async Task<Result<int>> Handle(GetUnreadCountQuery req, CancellationToken ct)
    {
        // Get all conversations the user is part of
        var participations = await tenantUow.Repository<ConversationParticipant>()
            .Query()
            .Where(p => p.EmployeeId == req.EmployeeId)
            .Include(p => p.Conversation)
                .ThenInclude(c => c.Messages)
            .ToListAsync(ct);

        var totalUnread = 0;

        foreach (var participation in participations)
        {
            if (participation.LastReadAt.HasValue)
            {
                totalUnread += participation.Conversation.Messages
                    .Count(m => m.SentAt > participation.LastReadAt.Value
                               && m.SenderId != req.EmployeeId
                               && !m.IsDeleted);
            }
            else
            {
                totalUnread += participation.Conversation.Messages
                    .Count(m => m.SenderId != req.EmployeeId && !m.IsDeleted);
            }
        }

        return Result<int>.Ok(totalUnread);
    }
}

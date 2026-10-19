using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Notifications.Commands;

internal sealed class MarkNotificationsReadHandler(ITenantUnitOfWork tenantUow, ICurrentUserService currentUser)
    : IRequestHandler<MarkNotificationsReadCommand, Result>
{
    public async Task<Result> Handle(MarkNotificationsReadCommand req, CancellationToken ct)
    {
        if (currentUser.GetUserId() is not { } userId)
        {
            return Result.Fail("User is not authenticated");
        }

        // One statement, so marking the same notification from two tabs at once cannot hit the
        // unique (notification, user) index; unknown ids are skipped by the join.
        await tenantUow.ExecuteSqlAsync($"""
            INSERT INTO notification_reads (id, notification_id, user_id, read_at)
            SELECT gen_random_uuid(), n.id, {userId}, {DateTime.UtcNow}
            FROM notifications n
            WHERE n.id = ANY({req.Ids.ToArray()})
            ON CONFLICT (notification_id, user_id) DO NOTHING
            """, ct);

        return Result.Ok();
    }
}

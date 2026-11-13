using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.Context;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.Notifications.Queries;

internal sealed class GetNotificationsHandler(ITenantUnitOfWork tenantUow, ICurrentUserService currentUser)
    : IRequestHandler<GetNotificationsQuery, Result<NotificationDto[]>>
{
    public async Task<Result<NotificationDto[]>> Handle(
        GetNotificationsQuery req,
        CancellationToken ct)
    {
        var userId = currentUser.GetUserId();
        var reads = tenantUow.Repository<NotificationRead>().Query();
        var notifications = await tenantUow.Repository<Notification>().Query()
            .Where(n => n.CreatedAt >= req.StartDate && n.CreatedAt <= req.EndDate)
            .OrderByDescending(n => n.CreatedAt)
            .Select(n => new { Notification = n, IsRead = reads.Any(r => r.NotificationId == n.Id && r.UserId == userId) })
            .ToListAsync(ct);

        return Result<NotificationDto[]>.Ok(notifications.Select(x => x.Notification.ToDto(x.IsRead)).ToArray());
    }
}

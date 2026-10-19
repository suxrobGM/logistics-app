using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Platform.Notifications.Queries;

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
            .Where(n => n.CreatedDate >= req.StartDate && n.CreatedDate <= req.EndDate)
            .OrderByDescending(n => n.CreatedDate)
            .Select(n => new { Notification = n, IsRead = reads.Any(r => r.NotificationId == n.Id && r.UserId == userId) })
            .ToListAsync(ct);

        return Result<NotificationDto[]>.Ok(notifications.Select(x => x.Notification.ToDto(x.IsRead)).ToArray());
    }
}

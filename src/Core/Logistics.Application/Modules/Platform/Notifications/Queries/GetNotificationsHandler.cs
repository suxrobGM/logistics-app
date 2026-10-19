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
        var notifications = await tenantUow.Repository<Notification>().Query()
            .Where(n => n.CreatedDate >= req.StartDate && n.CreatedDate <= req.EndDate)
            .OrderByDescending(n => n.CreatedDate)
            .ToListAsync(ct);

        var userId = currentUser.GetUserId();
        var ids = notifications.Select(n => n.Id).ToList();
        var readIds = await tenantUow.Repository<NotificationRead>().Query()
            .Where(r => r.UserId == userId && ids.Contains(r.NotificationId))
            .Select(r => r.NotificationId)
            .ToListAsync(ct);

        var read = readIds.ToHashSet();
        return Result<NotificationDto[]>.Ok(notifications.Select(n => n.ToDto(read.Contains(n.Id))).ToArray());
    }
}

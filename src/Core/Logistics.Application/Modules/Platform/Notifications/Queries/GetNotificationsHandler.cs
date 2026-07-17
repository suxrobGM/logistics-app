using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Notifications.Queries;

internal sealed class GetNotificationsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetNotificationsQuery, Result<NotificationDto[]>>
{
    public async Task<Result<NotificationDto[]>> Handle(
        GetNotificationsQuery req,
        CancellationToken ct)
    {
        var notificationsList =
            await tenantUow.Repository<Notification>()
                .GetListAsync(i => i.CreatedDate >= req.StartDate && i.CreatedDate <= req.EndDate, ct);

        var notificationsDto = notificationsList
            .Select(i => i.ToDto())
            .OrderByDescending(i => i.CreatedDate)
            .ToArray();

        return Result<NotificationDto[]>.Ok(notificationsDto);
    }
}

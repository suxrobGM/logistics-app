using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetNotificationsHandler : RequestHandler<GetNotificationsQuery, Result<NotificationDto[]>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetNotificationsHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public override async Task<Result<NotificationDto[]>> Handle(
        GetNotificationsQuery req,
        CancellationToken ct)
    {
        var notificationsList =
            await _tenantUow.Repository<Notification>()
                .GetListAsync(i => i.CreatedDate >= req.StartDate && i.CreatedDate <= req.EndDate, ct);

        var notificationsDto = notificationsList
            .Select(i => i.ToDto())
            .OrderByDescending(i => i.CreatedDate)
            .ToArray();

        return Result<NotificationDto[]>.Succeed(notificationsDto);
    }
}

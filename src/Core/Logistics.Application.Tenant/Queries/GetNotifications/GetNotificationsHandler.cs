using Logistics.Application.Tenant.Mappers;
using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetNotificationsHandler : RequestHandler<GetNotificationsQuery, ResponseResult<NotificationDto[]>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetNotificationsHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<NotificationDto[]>> HandleValidated(
        GetNotificationsQuery req, 
        CancellationToken cancellationToken)
    {
        var notificationsList =
            await _tenantRepository.GetListAsync<Notification>(i => i.CreatedDate >= req.StartDate && i.CreatedDate <= req.EndDate);

        var notificationsDto = notificationsList
            .Select(i => i.ToDto())
            .OrderByDescending(i => i.CreatedDate)
            .ToArray();
        
        return ResponseResult<NotificationDto[]>.CreateSuccess(notificationsDto);
    }
}

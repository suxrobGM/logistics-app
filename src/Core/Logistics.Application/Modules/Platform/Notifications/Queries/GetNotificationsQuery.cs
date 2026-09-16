using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Notifications.Queries;

public class GetNotificationsQuery : IntervalQuery, IQuery<Result<NotificationDto[]>>
{
}

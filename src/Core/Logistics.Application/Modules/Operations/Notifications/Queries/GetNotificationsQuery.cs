using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Notifications.Queries;

public class GetNotificationsQuery : IntervalQuery, IQuery<Result<NotificationDto[]>>
{
}

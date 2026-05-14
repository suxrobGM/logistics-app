using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetNotificationsQuery : IntervalQuery, IQuery<Result<NotificationDto[]>>
{
}

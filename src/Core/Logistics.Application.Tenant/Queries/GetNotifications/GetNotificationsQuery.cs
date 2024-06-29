using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Tenant.Queries;

public class GetNotificationsQuery : IntervalQuery, IRequest<Result<NotificationDto[]>>
{
}

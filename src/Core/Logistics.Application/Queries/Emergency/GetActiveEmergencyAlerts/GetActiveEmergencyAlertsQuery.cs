using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetActiveEmergencyAlertsQuery : SearchableQuery, IAppRequest<PagedResult<EmergencyAlertDto>>
{
}

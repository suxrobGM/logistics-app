using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetEmergencyAlertsQuery : SearchableQuery, IAppRequest<PagedResult<EmergencyAlertDto>>
{
    public Guid? DriverId { get; set; }
    public EmergencyAlertStatus? Status { get; set; }
    public EmergencyAlertType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

using Logistics.Application.Abstractions;
using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Dvir.Queries;

[RequiresFeature(TenantFeature.Dvir)]
public class GetDvirReportsQuery : SearchableQuery, IQuery<PagedResult<DvirReportDto>>
{
    public Guid? TruckId { get; set; }
    public Guid? DriverId { get; set; }
    public DvirStatus? Status { get; set; }
    public DvirType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Dvir.Queries;

public class GetDvirReportsQuery : SearchableQuery, IQuery<PagedResult<DvirReportDto>>
{
    public Guid? TruckId { get; set; }
    public Guid? DriverId { get; set; }
    public DvirStatus? Status { get; set; }
    public DvirType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

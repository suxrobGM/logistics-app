using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetAccidentReportsQuery : SearchableQuery, IAppRequest<PagedResult<AccidentReportDto>>
{
    public Guid? TruckId { get; set; }
    public Guid? DriverId { get; set; }
    public AccidentReportStatus? Status { get; set; }
    public AccidentSeverity? Severity { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

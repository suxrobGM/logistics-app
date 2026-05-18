using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums.Maintenance;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Maintenance.Queries;

public class GetMaintenanceRecordsQuery : SearchableQuery, IQuery<PagedResult<MaintenanceRecordDto>>
{
    public Guid? TruckId { get; set; }
    public MaintenanceType? Type { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

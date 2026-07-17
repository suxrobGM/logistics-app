using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Maintenance.Queries;

public class GetMaintenanceRecordByIdQuery : IQuery<Result<MaintenanceRecordDto>>, IHaveId
{
    public Guid Id { get; set; }
}

using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetMaintenanceRecordByIdQuery : IAppRequest<Result<MaintenanceRecordDto>>
{
    public Guid Id { get; set; }
}

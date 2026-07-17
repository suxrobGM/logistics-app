using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Safety.Queries;

public class GetDriverBehaviorEventByIdQuery : IQuery<Result<DriverBehaviorEventDto>>, IHaveId
{
    public Guid Id { get; set; }
}

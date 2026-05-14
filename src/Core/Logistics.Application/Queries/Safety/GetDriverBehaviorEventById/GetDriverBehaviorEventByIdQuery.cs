using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetDriverBehaviorEventByIdQuery : IQuery<Result<DriverBehaviorEventDto>>
{
    public Guid Id { get; set; }
}

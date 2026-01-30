using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetDriverBehaviorEventByIdQuery : IAppRequest<Result<DriverBehaviorEventDto>>
{
    public Guid Id { get; set; }
}

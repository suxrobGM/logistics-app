using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetContainerByIdQuery : IAppRequest<Result<ContainerDto>>
{
    public Guid Id { get; set; }
}

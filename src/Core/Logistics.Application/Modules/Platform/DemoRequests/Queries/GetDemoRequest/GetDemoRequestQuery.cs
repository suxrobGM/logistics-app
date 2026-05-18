using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.DemoRequests.Queries;

public sealed class GetDemoRequestQuery : IQuery<Result<DemoRequestDto>>
{
    public Guid Id { get; set; }
}

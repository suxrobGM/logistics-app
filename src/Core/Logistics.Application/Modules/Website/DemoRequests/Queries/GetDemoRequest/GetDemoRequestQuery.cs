using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Website.DemoRequests.Queries;

public sealed class GetDemoRequestQuery : IQuery<Result<DemoRequestDto>>
{
    public Guid Id { get; set; }
}

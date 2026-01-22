using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetDemoRequestQuery : IAppRequest<Result<DemoRequestDto>>
{
    public Guid Id { get; set; }
}

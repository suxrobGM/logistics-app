using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetDemoRequestQuery : IQuery<Result<DemoRequestDto>>
{
    public Guid Id { get; set; }
}

using Logistics.Application;
using Logistics.Shared.Models;
using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Queries;

public sealed class GetTenantQuery : IRequest<Result<TenantDto>>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public bool IncludeConnectionString { get; set; }
}

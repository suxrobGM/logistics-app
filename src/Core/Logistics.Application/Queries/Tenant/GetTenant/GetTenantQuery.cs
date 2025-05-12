using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public sealed class GetTenantQuery : IRequest<Result<TenantDto>>
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public bool IncludeConnectionString { get; set; }
}

using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public sealed class GetTenantQuery : IAppRequest<Result<TenantDto>>
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public bool IncludeConnectionString { get; set; }
}

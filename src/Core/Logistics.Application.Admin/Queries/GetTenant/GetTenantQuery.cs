using Logistics.Application.Common;
using Logistics.Shared.Models;
using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Queries;

public sealed class GetTenantQuery : IRequest<ResponseResult<TenantDto>>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public bool IncludeConnectionString { get; set; }
}

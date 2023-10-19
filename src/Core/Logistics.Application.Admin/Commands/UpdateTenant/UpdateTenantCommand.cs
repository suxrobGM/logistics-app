using Logistics.Application.Core;
using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class UpdateTenantCommand : IRequest<ResponseResult>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? ConnectionString { get; set; }
}

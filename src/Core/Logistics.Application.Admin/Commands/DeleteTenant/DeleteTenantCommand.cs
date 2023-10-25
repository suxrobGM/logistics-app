using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class DeleteTenantCommand : IRequest<ResponseResult>
{
    public string? Id { get; set; }
}

using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class DeleteTruckCommand : IRequest<ResponseResult>
{
    public string? Id { get; set; }
}

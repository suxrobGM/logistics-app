using Logistics.Shared.Enums;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class ConfirmLoadStatusCommand : IRequest<ResponseResult>
{
    public string? DriverId { get; set; }
    public string? LoadId { get; set; }
    public LoadStatus? LoadStatus { get; set; }
}

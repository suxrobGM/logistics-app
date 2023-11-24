using Logistics.Shared.Enums;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class ConfirmLoadStatusCommand : IRequest<ResponseResult>
{
    public string DriverId { get; set; } = default!;
    public string LoadId { get; set; } = default!;
    public LoadStatus? LoadStatus { get; set; }
}

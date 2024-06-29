using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class ConfirmLoadStatusCommand : IRequest<Result>
{
    public string DriverId { get; set; } = default!;
    public string LoadId { get; set; } = default!;
    public LoadStatus? LoadStatus { get; set; }
}

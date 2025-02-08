using Logistics.Shared.Models;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class ConfirmLoadStatusCommand : IRequest<Result>
{
    public string DriverId { get; set; } = null!;
    public string LoadId { get; set; } = null!;
    public LoadStatus? LoadStatus { get; set; }
}

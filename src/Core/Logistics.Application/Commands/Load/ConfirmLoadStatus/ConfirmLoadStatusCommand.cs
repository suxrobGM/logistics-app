using Logistics.Shared.Models;
using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Commands;

public class ConfirmLoadStatusCommand : IRequest<Result>
{
    public Guid DriverId { get; set; }
    public Guid LoadId { get; set; }
    public LoadStatus? LoadStatus { get; set; }
}

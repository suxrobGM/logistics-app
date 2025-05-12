using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateLoadProximityCommand : IRequest<Result>
{
    public Guid LoadId { get; set; }
    public bool? CanConfirmPickUp { get; set; }
    public bool? CanConfirmDelivery { get; set; }
}

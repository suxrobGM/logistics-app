using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateLoadProximityCommand : IRequest<Result>
{
    public string LoadId { get; set; } = null!;
    public bool? CanConfirmPickUp { get; set; }
    public bool? CanConfirmDelivery { get; set; }
}

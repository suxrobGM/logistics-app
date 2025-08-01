using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class CreateTruckCommand : IRequest<Result>
{
    public string TruckNumber { get; set; } = null!;
    public TruckType TruckType { get; set; }
    public Guid MainDriverId { get; set; }
}

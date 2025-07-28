using Logistics.Shared.Consts;
using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Commands;

public class UpdateTruckCommand : IRequest<Result>
{
    public Guid Id { get; set; }
    public string? TruckNumber { get; set; }
    public TruckType? TruckType { get; set; }
    public TruckStatus? TruckStatus { get; set; }
    public Guid? MainDriverId { get; set; }
    public Guid? SecondaryDriverId { get; set; }
}

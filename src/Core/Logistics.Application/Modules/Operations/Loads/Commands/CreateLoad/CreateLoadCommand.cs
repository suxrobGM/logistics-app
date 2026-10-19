using Logistics.Shared.Models;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

public partial class CreateLoadCommand : ICommand
{
    public string Name { get; set; } = null!;
    public LoadType Type { get; set; }
    public Address OriginAddress { get; set; } = null!;
    public GeoPoint OriginLocation { get; set; } = null!;
    public Address DestinationAddress { get; set; } = null!;
    public GeoPoint DestinationLocation { get; set; } = null!;
    public decimal DeliveryCost { get; set; }
    public double Distance { get; set; }
    public Guid AssignedDispatcherId { get; set; }
    public Guid? AssignedTruckId { get; set; }
    public Guid CustomerId { get; set; }

    public LoadSource Source { get; set; } = LoadSource.Manual;
    public DateTime? RequestedPickupDate { get; set; }
    public DateTime? RequestedDeliveryDate { get; set; }
    public string? Notes { get; set; }
    public Guid? ContainerId { get; set; }
    public Guid? OriginTerminalId { get; set; }
    public Guid? DestinationTerminalId { get; set; }

    public HazmatClass? HazmatClass { get; set; }
    public string? UnNumber { get; set; }

    /// <summary>
    /// Vehicles carried by a <see cref="LoadType.Vehicle"/> load. Must be empty for other load types.
    /// </summary>
    public List<LoadVehicleInput> Vehicles { get; set; } = [];
}

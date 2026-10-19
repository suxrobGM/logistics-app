using Logistics.Application.Modules.Operations.Loads.Services;

namespace Logistics.Application.Modules.Operations.Trips.Commands;

internal static class CreateTripLoadCommandExtensions
{
    public static CreateLoadParameters ToCreateLoadParameters(
        this CreateTripLoadCommand load, Guid? truckId, Guid? tripId) =>
        new(
            load.Name,
            load.Type,
            (load.OriginAddress, load.OriginLocation),
            (load.DestinationAddress, load.DestinationLocation),
            load.DeliveryCost,
            load.Distance,
            load.CustomerId,
            truckId,
            load.AssignedDispatcherId,
            tripId,
            Source: load.Source,
            RequestedPickupDate: load.RequestedPickupDate,
            RequestedDeliveryDate: load.RequestedDeliveryDate,
            Notes: load.Notes,
            ContainerId: load.ContainerId,
            OriginTerminalId: load.OriginTerminalId,
            DestinationTerminalId: load.DestinationTerminalId,
            HazmatClass: load.HazmatClass,
            UnNumber: load.UnNumber,
            Vehicles: load.Vehicles.ToLoadVehicles(),
            FuelSurcharge: load.FuelSurcharge,
            Accessorials: load.Accessorials,
            FreightClass: load.FreightClass,
            NmfcCode: load.NmfcCode,
            PalletCount: load.PalletCount,
            PieceCount: load.PieceCount,
            Weight: load.Weight,
            Dimensions: load.Dimensions,
            IsStackable: load.IsStackable);
}

using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class CreateLoadHandler(ILoadService loadService)
    : IRequestHandler<CreateLoadCommand, Result>
{
    public async Task<Result> Handle(
        CreateLoadCommand req, CancellationToken ct)
    {
        var createLoadParameters = new CreateLoadParameters(
            req.Name,
            req.Type,
            (req.OriginAddress, req.OriginLocation),
            (req.DestinationAddress, req.DestinationLocation),
            req.DeliveryCost,
            req.Distance,
            req.CustomerId,
            req.AssignedTruckId,
            req.AssignedDispatcherId,
            Source: req.Source,
            RequestedPickupDate: req.RequestedPickupDate,
            RequestedDeliveryDate: req.RequestedDeliveryDate,
            Notes: req.Notes,
            ContainerId: req.ContainerId,
            OriginTerminalId: req.OriginTerminalId,
            DestinationTerminalId: req.DestinationTerminalId,
            HazmatClass: req.HazmatClass,
            UnNumber: req.UnNumber,
            Vehicles: req.Vehicles.ToLoadVehicles(),
            FuelSurcharge: req.FuelSurcharge,
            Accessorials: req.Accessorials,
            FreightClass: req.FreightClass,
            NmfcCode: req.NmfcCode,
            PalletCount: req.PalletCount,
            PieceCount: req.PieceCount,
            Weight: req.Weight,
            Dimensions: req.Dimensions,
            IsStackable: req.IsStackable);

        // Load.Create() raises domain events for notifications:
        // - NewLoadCreatedEvent (always)
        // - LoadAssignedToTruckEvent (if truck assigned)
        var created = await loadService.CreateLoadAsync(createLoadParameters, ct: ct);
        return created.IsSuccess ? Result.Ok() : Result.Fail(created.Error!, created.ErrorCode!);
    }
}

using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

internal sealed class CreateLoadHandler(ILoadService loadService)
    : IAppRequestHandler<CreateLoadCommand, Result>
{
    public async Task<Result> Handle(
        CreateLoadCommand req, CancellationToken ct)
    {
        try
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
                IsHazmat: req.IsHazmat,
                HazmatClass: req.HazmatClass,
                UnNumber: req.UnNumber);

            // Load.Create() raises domain events for notifications:
            // - NewLoadCreatedEvent (always)
            // - LoadAssignedToTruckEvent (if truck assigned)
            await loadService.CreateLoadAsync(createLoadParameters, ct: ct);

            return Result.Ok();
        }
        catch (InvalidOperationException e)
        {
            return Result.Fail(e.Message);
        }
    }
}

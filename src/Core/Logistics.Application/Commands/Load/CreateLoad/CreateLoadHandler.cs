using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

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
                req.AssignedDispatcherId);

            // Load.Create() raises domain events for notifications:
            // - NewLoadCreatedEvent (always)
            // - LoadAssignedToTruckEvent (if truck assigned)
            await loadService.CreateLoadAsync(createLoadParameters);

            return Result.Ok();
        }
        catch (InvalidOperationException e)
        {
            return Result.Fail(e.Message);
        }
    }
}

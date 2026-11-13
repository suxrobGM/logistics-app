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
        // Load.Create() raises domain events for notifications:
        // - NewLoadCreatedEvent (always)
        // - LoadAssignedToTruckEvent (if truck assigned)
        var created = await loadService.CreateLoadAsync(new NewLoad(req, req.AssignedTruckId), ct: ct);
        return created.IsSuccess ? Result.Ok() : Result.Fail(created);
    }
}

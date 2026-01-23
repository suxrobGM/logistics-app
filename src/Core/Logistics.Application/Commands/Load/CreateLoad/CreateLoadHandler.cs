using Logistics.Application.Abstractions;
using Logistics.Application.Extensions;
using Logistics.Application.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateLoadHandler(
    ILoadService loadService,
    IPushNotificationService pushNotificationService,
    INotificationService notificationService)
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

            var newLoad = await loadService.CreateLoadAsync(createLoadParameters);

            // Send push notification to driver (if truck assigned)
            await pushNotificationService.SendNewLoadNotificationAsync(newLoad);

            // Send in-app notification for TMS portal users (if truck assigned)
            if (newLoad.AssignedTruck is not null)
            {
                var driverName = newLoad.AssignedTruck.MainDriver?.GetFullName() ?? newLoad.AssignedTruck.Number;
                await notificationService.SendNotificationAsync(
                    "New load created",
                    $"Load #{newLoad.Number} has been created and assigned to {driverName}");
            }

            return Result.Ok();
        }
        catch (InvalidOperationException e)
        {
            return Result.Fail(e.Message);
        }
    }
}

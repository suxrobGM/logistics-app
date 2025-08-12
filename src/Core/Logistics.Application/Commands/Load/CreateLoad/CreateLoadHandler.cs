using Logistics.Application.Abstractions;
using Logistics.Application.Extensions;
using Logistics.Application.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateLoadHandler : IAppRequestHandler<CreateLoadCommand, Result>
{
    private readonly ILoadService _loadService;
    private readonly IPushNotificationService _pushNotificationService;

    public CreateLoadHandler(
        ILoadService loadService,
        IPushNotificationService pushNotificationService)
    {
        _loadService = loadService;
        _pushNotificationService = pushNotificationService;
    }

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

            var newLoad = await _loadService.CreateLoadAsync(createLoadParameters);

            await _pushNotificationService.SendNewLoadNotificationAsync(newLoad);
            return Result.Succeed();
        }
        catch (InvalidOperationException e)
        {
            return Result.Fail(e.Message);
        }
    }
}

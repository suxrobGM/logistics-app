using Logistics.Application.Tenant.Services;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateLoadHandler : RequestHandler<CreateLoadCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly IPushNotificationService _pushNotificationService;

    public CreateLoadHandler(
        ITenantRepository tenantRepository,
        IPushNotificationService pushNotificationService)
    {
        _tenantRepository = tenantRepository;
        _pushNotificationService = pushNotificationService;
    }

    protected override async Task<ResponseResult> HandleValidated(
        CreateLoadCommand req, CancellationToken cancellationToken)
    {
        var dispatcher = await _tenantRepository.GetAsync<Employee>(req.AssignedDispatcherId);

        if (dispatcher == null)
            return ResponseResult.CreateError("Could not find the specified dispatcher");

        var truck = await _tenantRepository.GetAsync<Truck>(req.AssignedTruckId);

        if (truck == null)
            return ResponseResult.CreateError($"Could not find the truck with ID '{req.AssignedTruckId}'");
        
        var latestLoad = _tenantRepository.Query<Load>().OrderBy(i => i.RefId).LastOrDefault();
        ulong refId = 1000;

        if (latestLoad != null)
            refId = latestLoad.RefId + 1;
        
        var load = Load.Create(
            refId, 
            req.OriginAddress!,
            req.OriginAddressLat,
            req.OriginAddressLong,
            req.DestinationAddress!,
            req.DestinationAddressLat,
            req.DestinationAddressLong,
            truck, 
            dispatcher);
        
        load.Name = req.Name;
        load.Distance = req.Distance;
        load.DeliveryCost = req.DeliveryCost;

        await _tenantRepository.AddAsync(load);
        await _tenantRepository.UnitOfWork.CommitAsync();
        await SendNotificationsToDriversAsync(load, truck);
        return ResponseResult.CreateSuccess();
    }

    private async Task SendNotificationsToDriversAsync(Load load, Truck truck)
    {
        var drivers = truck.Drivers.Where(driver => !string.IsNullOrEmpty(driver.DeviceToken));
        
        foreach (var driver in drivers)
        {
            await _pushNotificationService.SendNotificationAsync(
                "Received a load",
                $"A new load #{load.RefId} has been assigned to you", 
                driver.DeviceToken!,
                new Dictionary<string, string> {{"loadId", load.Id}});
        }
    }
}

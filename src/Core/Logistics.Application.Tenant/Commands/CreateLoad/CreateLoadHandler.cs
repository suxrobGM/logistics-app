using Logistics.Application.Tenant.Services;

namespace Logistics.Application.Tenant.Commands;

internal sealed class CreateLoadHandler : RequestHandler<CreateLoadCommand, ResponseResult>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;
    private readonly IPushNotificationService _pushNotificationService;

    public CreateLoadHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository,
        IPushNotificationService pushNotificationService)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
        _pushNotificationService = pushNotificationService;
    }

    protected override async Task<ResponseResult> HandleValidated(
        CreateLoadCommand request, CancellationToken cancellationToken)
    {
        var dispatcher = await _tenantRepository.GetAsync<Employee>(request.AssignedDispatcherId);
        var driver = await _tenantRepository.GetAsync<Employee>(request.AssignedDriverId);

        if (dispatcher == null)
            return ResponseResult.CreateError("Could not find the specified dispatcher");

        if (driver == null)
            return ResponseResult.CreateError("Could not find the specified driver");

        var truck = await _tenantRepository.GetAsync<Truck>(i => i.DriverId == driver.Id);

        if (truck == null)
        {
            var user = await _mainRepository.GetAsync<User>(request.AssignedDriverId);
            return ResponseResult.CreateError($"Could not find the truck whose driver is '{user?.GetFullName()}'");
        }
        
        var latestLoad = _tenantRepository.Query<Load>().OrderBy(i => i.RefId).LastOrDefault();
        ulong refId = 100_000;

        if (latestLoad != null)
            refId = latestLoad.RefId + 1;
        
        var loadEntity = Load.CreateLoad(refId, request.SourceAddress!, request.DestinationAddress!, truck, dispatcher);
        loadEntity.Name = request.Name;
        loadEntity.Distance = request.Distance;
        loadEntity.DeliveryCost = request.DeliveryCost;

        await _tenantRepository.AddAsync(loadEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();

        if (!string.IsNullOrEmpty(driver.DeviceToken))
        {
            await _pushNotificationService.SendNotificationAsync(
                "Received a load",
                $"A new load #{loadEntity.RefId} has been assigned to you", 
                driver.DeviceToken,
                new Dictionary<string, string> {{"loadId", loadEntity.Id}});
        }
        
        return ResponseResult.CreateSuccess();
    }
}

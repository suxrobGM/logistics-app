using Logistics.Application.Tenant.Extensions;
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

        if (dispatcher is null)
            return ResponseResult.CreateError("Could not find the specified dispatcher");

        var truck = await _tenantRepository.GetAsync<Truck>(req.AssignedTruckId);

        if (truck is null)
            return ResponseResult.CreateError($"Could not find the truck with ID '{req.AssignedTruckId}'");

        var customer = await _tenantRepository.GetAsync<Customer>(req.CustomerId);
        
        if (customer is null)
            return ResponseResult.CreateError($"Could not find the customer with ID '{req.CustomerId}'");
        
        var latestLoad = _tenantRepository.Query<Load>().OrderBy(i => i.RefId).LastOrDefault();
        ulong refId = 1000;

        if (latestLoad != null)
            refId = latestLoad.RefId + 1;
        
        var load = Load.Create(
            refId,
            req.DeliveryCost,
            req.OriginAddress!,
            req.OriginAddressLat,
            req.OriginAddressLong,
            req.DestinationAddress!,
            req.DestinationAddressLat,
            req.DestinationAddressLong,
            customer,
            truck, 
            dispatcher);
        
        load.Name = req.Name;
        load.Distance = req.Distance;

        await _tenantRepository.AddAsync(load);
        var changes = await _tenantRepository.UnitOfWork.CommitAsync();

        if (changes > 0)
        {
            await _pushNotificationService.SendNewLoadNotificationAsync(load, truck);
        }
        return ResponseResult.CreateSuccess();
    }
}

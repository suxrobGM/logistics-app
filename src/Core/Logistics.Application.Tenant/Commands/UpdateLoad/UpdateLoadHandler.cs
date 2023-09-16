namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateLoadHandler : RequestHandler<UpdateLoadCommand, ResponseResult>
{
    private readonly ITenantRepository _tenantRepository;

    public UpdateLoadHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(
        UpdateLoadCommand req, CancellationToken cancellationToken)
    {
        var loadEntity = await _tenantRepository.GetAsync<Load>(req.Id);

        if (loadEntity == null)
            return ResponseResult.CreateError("Could not find the specified load");
        
        if (req.AssignedTruckId != null)
        {
            var truck = await _tenantRepository.GetAsync<Truck>(req.AssignedTruckId);

            if (truck == null)
            {
                return ResponseResult.CreateError($"Could not find a truck with ID is '{req.AssignedTruckId}'");
            }

            loadEntity.AssignedTruck = truck;
        }

        if (req.AssignedDispatcherId != null)
        {
            var dispatcher = await _tenantRepository.GetAsync<Employee>(req.AssignedDispatcherId);

            if (dispatcher == null)
            {
                return ResponseResult.CreateError($"Could not find a dispatcher with ID is '{req.AssignedDispatcherId}'");
            }

            loadEntity.AssignedDispatcher = dispatcher;
        }

        if (req.Name != null)
            loadEntity.Name = req.Name;

        if (req.OriginAddress != null)
        {
            loadEntity.OriginAddress = req.OriginAddress;
            loadEntity.OriginAddressLat = req.OriginAddressLat;
            loadEntity.OriginAddressLong = req.OriginAddressLong;
        }

        if (req.DestinationAddress != null)
        {
            loadEntity.DestinationAddress = req.DestinationAddress;
            loadEntity.DestinationAddressLat = req.DestinationAddressLat;
            loadEntity.DestinationAddressLong = req.DestinationAddressLong;
        }
        
        if (req.DeliveryCost.HasValue)
            loadEntity.DeliveryCost = req.DeliveryCost.Value;
        
        if (req.Distance.HasValue)
            loadEntity.Distance = req.Distance.Value;

        if (req.Status.HasValue)
            loadEntity.Status = req.Status.Value;  
        
        _tenantRepository.Update(loadEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}

namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateLoadHandler : RequestHandler<UpdateLoadCommand, ResponseResult>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public UpdateLoadHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
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
                return ResponseResult.CreateError($"Could not find the truck with ID is '{req.AssignedTruckId}'");
            }

            loadEntity.AssignedTruck = truck;
        }

        if (req.Name != null)
            loadEntity.Name = req.Name;
        
        if (req.SourceAddress != null)
            loadEntity.OriginAddress = req.SourceAddress;
        
        if (req.DestinationAddress != null)
            loadEntity.DestinationAddress = req.DestinationAddress;
        
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

    protected override bool Validate(UpdateLoadCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }
        else if (request.DeliveryCost is < 0)
        {
            errorDescription = "Delivery cost should be non-negative value";
        }
        else if (request.Distance is < 0)
        {
            errorDescription = "Distance should be non-negative value";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}

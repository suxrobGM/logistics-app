namespace Logistics.Application.Tenant.Commands;

internal sealed class UpdateLoadHandler : RequestHandlerBase<UpdateLoadCommand, ResponseResult>
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
        UpdateLoadCommand request, CancellationToken cancellationToken)
    {
        Employee? driver = null;
        Truck? truck = null;
        
        if (request.AssignedDriverId != null)
        {
            driver = await _tenantRepository.GetAsync<Employee>(request.AssignedDriverId);

            if (driver == null)
                return ResponseResult.CreateError("Could not find the specified driver");
            
            truck = await _tenantRepository.GetAsync<Truck>(i => i.DriverId == driver.Id);

            if (truck == null)
            {
                var user = await _mainRepository.GetAsync<User>(request.AssignedDriverId);
                return ResponseResult.CreateError($"Could not find the truck whose driver is '{user?.GetFullName()}'");
            }
        }

        var loadEntity = await _tenantRepository.GetAsync<Load>(request.Id);

        if (loadEntity == null)
            return ResponseResult.CreateError("Could not find the specified load");
        
        if (driver != null && truck != null)
        {
            loadEntity.AssignedTruck = truck;
            loadEntity.AssignedDriver = driver;
        }

        if (request.Name != null)
            loadEntity.Name = request.Name;
        
        if (request.SourceAddress != null)
            loadEntity.SourceAddress = request.SourceAddress;
        
        if (request.DestinationAddress != null)
            loadEntity.DestinationAddress = request.DestinationAddress;
        
        if (request.DeliveryCost.HasValue)
            loadEntity.DeliveryCost = request.DeliveryCost.Value;
        
        if (request.Distance.HasValue)
            loadEntity.Distance = request.Distance.Value;

        if (request.Status.HasValue)
            loadEntity.Status = request.Status.Value;  
        
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

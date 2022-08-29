namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateLoadHandler : RequestHandlerBase<UpdateLoadCommand, DataResult>
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

    protected override async Task<DataResult> HandleValidated(
        UpdateLoadCommand request, CancellationToken cancellationToken)
    {
        var driverEmp = await _tenantRepository.GetAsync<Employee>(request.AssignedDriverId);
        var driverUser = await _mainRepository.GetAsync<User>(request.AssignedDriverId);

        if (driverEmp == null)
            return DataResult.CreateError("Could not find the specified driver");
        
        var truck = await _tenantRepository.GetAsync<Truck>(i => i.DriverId == driverEmp.Id);

        if (truck == null)
            return DataResult.CreateError($"Could not find the truck whose driver is '{driverUser?.UserName}'");

        var loadEntity = await _tenantRepository.GetAsync<Load>(request.Id);

        if (loadEntity == null)
            return DataResult.CreateError("Could not find the specified load");
        
        loadEntity.Name = request.Name;
        loadEntity.SourceAddress = request.SourceAddress;
        loadEntity.DestinationAddress = request.DestinationAddress;
        loadEntity.Distance = request.Distance;
        loadEntity.DeliveryCost = request.DeliveryCost;
        loadEntity.Status = LoadStatus.Get(request.Status)!;
        loadEntity.AssignedDriver = driverEmp;
        loadEntity.AssignedTruck = truck;

        _tenantRepository.Update(loadEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(UpdateLoadCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }
        else if (string.IsNullOrEmpty(request.SourceAddress))
        {
            errorDescription = "Source address is an empty string";
        }
        else if (string.IsNullOrEmpty(request.DestinationAddress))
        {
            errorDescription = "Destination address is an empty string";
        }
        else if (LoadStatus.Get(request.Status) == null)
        {
            errorDescription = $"The'{request.Status}' is an invalid value of the status property";
        }
        else if (string.IsNullOrEmpty(request.AssignedDriverId))
        {
            errorDescription = "AssignedDriverId is an empty string";
        }
        else if (request.DeliveryCost < 0)
        {
            errorDescription = "Delivery cost should be non-negative value";
        }
        else if (request.Distance < 0)
        {
            errorDescription = "Distance miles should be non-negative value";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}

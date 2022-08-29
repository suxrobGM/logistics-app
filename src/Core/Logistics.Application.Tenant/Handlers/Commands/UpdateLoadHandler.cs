namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateLoadHandler : RequestHandlerBase<UpdateLoadCommand, DataResult>
{
    private readonly ITenantRepository<Employee> _employeeRepository;
    private readonly ITenantRepository<Load> _loadRepository;
    private readonly ITenantRepository<Truck> _truckRepository;

    public UpdateLoadHandler(
        ITenantRepository<Employee> employeeRepository,
        ITenantRepository<Load> loadRepository,
        ITenantRepository<Truck> truckRepository)
    {
        _employeeRepository = employeeRepository;
        _loadRepository = loadRepository;
        _truckRepository = truckRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        UpdateLoadCommand request, CancellationToken cancellationToken)
    {
        var driver = await _employeeRepository.GetAsync(request.AssignedDriverId!);

        if (driver == null)
            return DataResult.CreateError("Could not find the specified driver");
        
        var truck = await _truckRepository.GetAsync(i => i.DriverId == driver.Id);

        if (truck == null)
            return DataResult.CreateError($"Could not find the truck whose driver ID is '{driver.Id}'");

        var loadEntity = await _loadRepository.GetAsync(request.Id!);

        if (loadEntity == null)
            return DataResult.CreateError("Could not find the specified load");
        
        loadEntity.Name = request.Name;
        loadEntity.SourceAddress = request.SourceAddress;
        loadEntity.DestinationAddress = request.DestinationAddress;
        loadEntity.Distance = request.Distance;
        loadEntity.DeliveryCost = request.DeliveryCost;
        loadEntity.Status = LoadStatus.Get(request.Status)!;
        loadEntity.AssignedDriver = driver;
        loadEntity.AssignedTruck = truck;

        _loadRepository.Update(loadEntity);
        await _loadRepository.UnitOfWork.CommitAsync();
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

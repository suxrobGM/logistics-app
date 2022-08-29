namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateLoadHandler : RequestHandlerBase<CreateLoadCommand, DataResult>
{
    private readonly ITenantRepository<Load> _loadRepository;
    private readonly ITenantRepository<Truck> _truckRepository;
    private readonly ITenantRepository<Employee> _employeeRepository;

    public CreateLoadHandler(
        ITenantRepository<Load> loadRepository,
        ITenantRepository<Truck> truckRepository,
        ITenantRepository<Employee> employeeRepository)
    {
        _loadRepository = loadRepository;
        _truckRepository = truckRepository;
        _employeeRepository = employeeRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateLoadCommand request, CancellationToken cancellationToken)
    {
        var dispatcher = await _employeeRepository.GetAsync(request.AssignedDispatcherId!);

        if (dispatcher == null)
            return DataResult.CreateError("Could not find the specified dispatcher");
        
        var driver = await _employeeRepository.GetAsync(request.AssignedDriverId!);

        if (driver == null)
            return DataResult.CreateError("Could not find the specified driver");

        var truck = await _truckRepository.GetAsync(i => i.DriverId == driver.Id);

        if (truck == null)
            return DataResult.CreateError($"Could not find the truck whose driver ID is '{driver.Id}'");
        
        var latestLoad = _loadRepository.GetQuery().OrderBy(i => i.ReferenceId).LastOrDefault();
        ulong refId = 100_000;

        if (latestLoad != null)
            refId = latestLoad.ReferenceId + 1;

        var loadEntity = new Load
        {
            ReferenceId = refId,
            Name = request.Name,
            SourceAddress = request.SourceAddress,
            Status = LoadStatus.Dispatched,
            DestinationAddress = request.DestinationAddress,
            DispatchedDate = request.DispatchedDate,
            Distance = request.Distance,
            DeliveryCost = request.DeliveryCost,
            AssignedDispatcher = dispatcher,
            AssignedDriver = driver,
            AssignedTruck = truck
        };

        await _loadRepository.AddAsync(loadEntity);
        await _loadRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(CreateLoadCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.AssignedDispatcherId))
        {
            errorDescription = "AssignedDispatcherId is an empty string";
        }
        else if (string.IsNullOrEmpty(request.AssignedDriverId))
        {
            errorDescription = "AssignedDriverId is an empty string";
        }
        else if (string.IsNullOrEmpty(request.SourceAddress))
        {
            errorDescription = "Source address is an empty string";
        }
        else if (string.IsNullOrEmpty(request.DestinationAddress))
        {
            errorDescription = "Destination address is an empty string";
        }
        else if (request.DeliveryCost < 0)
        {
            errorDescription = "Delivery cost should be non-negative value";
        }
        else if (request.Distance < 0)
        {
            errorDescription = "Distance should be non-negative value";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}

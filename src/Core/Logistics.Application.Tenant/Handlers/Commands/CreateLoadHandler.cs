namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateLoadHandler : RequestHandlerBase<CreateLoadCommand, DataResult>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public CreateLoadHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateLoadCommand request, CancellationToken cancellationToken)
    {
        var dispatcher = await _tenantRepository.GetAsync<Employee>(request.AssignedDispatcherId);
        var driver = await _tenantRepository.GetAsync<Employee>(request.AssignedDriverId);

        if (dispatcher == null)
            return DataResult.CreateError("Could not find the specified dispatcher");

        if (driver == null)
            return DataResult.CreateError("Could not find the specified driver");

        var truck = await _tenantRepository.GetAsync<Truck>(i => i.DriverId == driver.Id);

        if (truck == null)
        {
            var user = await _mainRepository.GetAsync<User>(request.AssignedDriverId);
            return DataResult.CreateError($"Could not find the truck whose driver ID is '{user?.UserName}'");
        }
        
        var latestLoad = _tenantRepository.Query<Load>().OrderBy(i => i.RefId).LastOrDefault();
        ulong refId = 100_000;

        if (latestLoad != null)
            refId = latestLoad.RefId + 1;

        var loadEntity = new Load
        {
            RefId = refId,
            Name = request.Name,
            SourceAddress = request.SourceAddress,
            Status = LoadStatus.Dispatched,
            DestinationAddress = request.DestinationAddress,
            Distance = request.Distance,
            DeliveryCost = request.DeliveryCost,
            AssignedDispatcher = dispatcher,
            AssignedDriver = driver,
            AssignedTruck = truck
        };

        await _tenantRepository.AddAsync(loadEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();
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

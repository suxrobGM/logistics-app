namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateTruckHandler : RequestHandlerBase<CreateTruckCommand, DataResult>
{
    private readonly ITenantRepository _tenantRepository;

    public CreateTruckHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateTruckCommand request, CancellationToken cancellationToken)
    {
        var driver = await _tenantRepository.GetAsync<Employee>(request.DriverId);

        if (driver == null)
            return DataResult.CreateError("Could not find the specified driver");

        var truckWithThisDriver = await _tenantRepository.GetAsync<Truck>(i => i.DriverId == request.DriverId);
        var truckWithThisNumber = await _tenantRepository.GetAsync<Truck>(i => i.TruckNumber == request.TruckNumber);

        if (truckWithThisDriver != null)
            return DataResult.CreateError("Already exists truck with this driver");

        if (truckWithThisNumber != null)
            return DataResult.CreateError("Already exists truck with this number");

        var truckEntity = new Truck()
        {
            TruckNumber = request.TruckNumber ?? 100,
            Driver = driver
        };
        
        await _tenantRepository.AddAsync(truckEntity);
        await _tenantRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(CreateTruckCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (request.TruckNumber is null)
        {
            errorDescription = "Truck number is not specified";
        }
        else if (request.TruckNumber < 0)
        {
            errorDescription = "Truck number should be non-negative";
        }
        else if (string.IsNullOrEmpty(request.DriverId))
        {
            errorDescription = "DriverId is not specified";
        }
        
        return string.IsNullOrEmpty(errorDescription);
    }
}

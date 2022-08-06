namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateTruckHandler : RequestHandlerBase<CreateTruckCommand, DataResult>
{
    private readonly ITenantRepository<Truck> _truckRepository;
    private readonly ITenantRepository<Employee> _employeeRepository;

    public CreateTruckHandler(
        ITenantRepository<Truck> truckRepository,
        ITenantRepository<Employee> employeeRepository)
    {
        _truckRepository = truckRepository;
        _employeeRepository = employeeRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateTruckCommand request, CancellationToken cancellationToken)
    {
        var driver = await _employeeRepository.GetAsync(i => i.Id == request.DriverId || i.ExternalId == request.DriverId);

        if (driver == null)
            return DataResult.CreateError("Could not find the specified driver");

        var truckWithThisDriver = await _truckRepository.GetAsync(i => i.DriverId == request.DriverId);
        var truckWithThisNumber = await _truckRepository.GetAsync(i => i.TruckNumber == request.TruckNumber);

        if (truckWithThisDriver != null)
            return DataResult.CreateError("Already exists truck with this driver");

        if (truckWithThisNumber != null)
            return DataResult.CreateError("Already exists truck with this number");

        var truckEntity = new Truck()
        {
            TruckNumber = request.TruckNumber,
            Driver = driver,
            DriverId = driver.ExternalId
        };
        
        await _truckRepository.AddAsync(truckEntity);
        await _truckRepository.UnitOfWork.CommitAsync();
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

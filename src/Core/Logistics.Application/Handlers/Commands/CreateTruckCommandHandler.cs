namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateTruckCommandHandler : RequestHandlerBase<CreateTruckCommand, DataResult>
{
    private readonly ITenantRepository<Truck> truckRepository;
    private readonly ITenantRepository<User> userRepository;

    public CreateTruckCommandHandler(
        ITenantRepository<Truck> truckRepository,
        ITenantRepository<User> userRepository)
    {
        this.truckRepository = truckRepository;
        this.userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateTruckCommand request, CancellationToken cancellationToken)
    {
        var driver = await userRepository.GetAsync(request.DriverId!);

        if (driver == null)
        {
            return DataResult.CreateError("Could not find the specified driver");
        }

        var truckWithThisDriver = await truckRepository.GetAsync(i => i.DriverId == request.DriverId);
        var truckWithThisNumber = await truckRepository.GetAsync(i => i.TruckNumber == request.TruckNumber);

        if (truckWithThisDriver != null)
        {
            return DataResult.CreateError("Already exists truck with this driver");
        }
        else if (truckWithThisNumber != null)
        {
            return DataResult.CreateError("Already exists truck with this number");
        }

        var truckEntity = new Truck()
        {
            TruckNumber = request.TruckNumber,
            Driver = driver,
        };
        
        await truckRepository.AddAsync(truckEntity);
        await truckRepository.UnitOfWork.CommitAsync();
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

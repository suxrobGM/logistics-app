namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateTruckCommandHandler : RequestHandlerBase<CreateTruckCommand, DataResult>
{
    private readonly IRepository<Truck> truckRepository;
    private readonly IRepository<User> userRepository;

    public CreateTruckCommandHandler(
        IRepository<Truck> truckRepository,
        IRepository<User> userRepository)
    {
        this.truckRepository = truckRepository;
        this.userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateTruckCommand request, CancellationToken cancellationToken)
    {
        User? driver = null;
        if (!string.IsNullOrEmpty(request.DriverId))
        {
            driver = await userRepository.GetAsync(request.DriverId);

            if (driver == null)
            {
                return DataResult.CreateError("Could not found the specified driver");
            }
        }

        var truckEntity = new Truck()
        {
            TruckNumber = request.TruckNumber,
        };

        if (driver != null)
        {
            truckEntity.Driver = driver;
        }
        
        await truckRepository.AddAsync(truckEntity);
        await truckRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(CreateTruckCommand request, out string errorDescription)
    {
        if (request.TruckNumber is null)
        {
            errorDescription = "Truck number is not specified";
            return false;
        }

        if (request.TruckNumber < 0)
        {
            errorDescription = "Truck number should be non-negative";
            return false;
        }

        errorDescription = string.Empty;
        return true;
    }
}

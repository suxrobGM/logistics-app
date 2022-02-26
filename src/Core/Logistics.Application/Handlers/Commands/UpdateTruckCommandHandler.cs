namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateTruckCommandHandler : RequestHandlerBase<UpdateTruckCommand, DataResult>
{
    private readonly IRepository<Truck> truckRepository;
    private readonly IRepository<User> userRepository;

    public UpdateTruckCommandHandler(
        IRepository<Truck> truckRepository,
        IRepository<User> userRepository)
    {
        this.truckRepository = truckRepository;
        this.userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        UpdateTruckCommand request, CancellationToken cancellationToken)
    {
        var driver = await userRepository.GetAsync(request.DriverId!);

        if (driver == null)
        {
            return DataResult.CreateError("Could not find the specified driver");
        }

        var truckEntity = await truckRepository.GetAsync(request.Id!);

        if (truckEntity == null)
        {
            return DataResult.CreateError("Could not find the specified truck");
        }

        truckEntity.Driver = driver;
        truckRepository.Update(truckEntity);
        await truckRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(UpdateTruckCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }
        else if (string.IsNullOrEmpty(request.DriverId))
        {
            errorDescription = "Truck driver id is an empty string";
        }
        
        return string.IsNullOrEmpty(errorDescription);
    }
}

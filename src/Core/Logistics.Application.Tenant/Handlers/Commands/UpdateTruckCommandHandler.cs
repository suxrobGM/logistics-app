namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateTruckCommandHandler : RequestHandlerBase<UpdateTruckCommand, DataResult>
{
    private readonly ITenantRepository<Truck> _truckRepository;
    private readonly ITenantRepository<User> _userRepository;

    public UpdateTruckCommandHandler(
        ITenantRepository<Truck> truckRepository,
        ITenantRepository<User> userRepository)
    {
        _truckRepository = truckRepository;
        _userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        UpdateTruckCommand request, CancellationToken cancellationToken)
    {
        var driver = await _userRepository.GetAsync(request.DriverId!);

        if (driver == null)
        {
            return DataResult.CreateError("Could not find the specified driver");
        }

        var truckEntity = await _truckRepository.GetAsync(request.Id!);

        if (truckEntity == null)
        {
            return DataResult.CreateError("Could not find the specified truck");
        }

        truckEntity.Driver = driver;
        _truckRepository.Update(truckEntity);
        await _truckRepository.UnitOfWork.CommitAsync();
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

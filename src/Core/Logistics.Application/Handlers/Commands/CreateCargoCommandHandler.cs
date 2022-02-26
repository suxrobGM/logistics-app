using Logistics.Domain.ValueObjects;

namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateCargoCommandHandler : RequestHandlerBase<CreateCargoCommand, DataResult>
{
    private readonly IRepository<Cargo> cargoRepository;
    private readonly IRepository<Truck> truckRepository;
    private readonly IRepository<User> userRepository;

    public CreateCargoCommandHandler(
        IRepository<Cargo> cargoRepository,
        IRepository<Truck> truckRepository,
        IRepository<User> userRepository)
    {
        this.cargoRepository = cargoRepository;
        this.truckRepository = truckRepository;
        this.userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateCargoCommand request, CancellationToken cancellationToken)
    {
        Truck? truck = null;
        var dispatcher = await userRepository.GetAsync(request.AssignedDispatcherId!);

        if (dispatcher == null)
        {
            return DataResult.CreateError("Could not found the dispatcher");
        }

        if (!string.IsNullOrEmpty(request.AssignedTruckId))
        {
            truck = await truckRepository.GetAsync(request.AssignedTruckId);

            if (truck == null)
            {
                return DataResult.CreateError("Could not found the specified truck");
            }
        }

        var cargoEntity = new Cargo
        {
            Source = request.Source,
            Status = CargoStatus.OffDuty,
            Destination = request.Destination,
            TotalTripMiles = request.TotalTripMiles,
            PricePerMile = request.PricePerMile,
            AssignedDispatcherId = dispatcher.Id
        };

        if (truck != null)
        {
            cargoEntity.AssignedTruck = truck;
        }

        await cargoRepository.AddAsync(cargoEntity);
        await cargoRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(CreateCargoCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.AssignedDispatcherId))
        {
            errorDescription = "Dispatcher Id is a empty string";
        }
        else if (string.IsNullOrEmpty(request.Source))
        {
            errorDescription = "Source address is a empty string";
        }
        else if (string.IsNullOrEmpty(request.Destination))
        {
            errorDescription = "Destination address is a empty string";
        }
        else if (string.IsNullOrEmpty(request.AssignedDispatcherId))
        {
            errorDescription = "AssignedDispatcherId is a empty string";
        }
        else if (request.PricePerMile < 0)
        {
            errorDescription = "Price per mile should be non-negative value";
        }
        else if (request.TotalTripMiles < 0)
        {
            errorDescription = "Total trip miles should be non-negative value";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}

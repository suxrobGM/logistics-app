using Logistics.Domain.ValueObjects;

namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateCargoCommandHandler : RequestHandlerBase<CreateCargoCommand, DataResult>
{
    private readonly ITenantRepository<Cargo> cargoRepository;
    private readonly ITenantRepository<Truck> truckRepository;
    private readonly ITenantRepository<User> userRepository;

    public CreateCargoCommandHandler(
        ITenantRepository<Cargo> cargoRepository,
        ITenantRepository<Truck> truckRepository,
        ITenantRepository<User> userRepository)
    {
        this.cargoRepository = cargoRepository;
        this.truckRepository = truckRepository;
        this.userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateCargoCommand request, CancellationToken cancellationToken)
    {
        var dispatcher = await userRepository.GetAsync(request.AssignedDispatcherId!);

        if (dispatcher == null)
        {
            return DataResult.CreateError("Could not find the specified dispatcher");
        }

        var truck = await truckRepository.GetAsync(request.AssignedTruckId!);
        if (truck == null)
        {
            return DataResult.CreateError("Could not find the specified truck driver");
        }

        var cargoEntity = new Cargo
        {
            Source = request.Source,
            Status = CargoStatus.Ready,
            Destination = request.Destination,
            TotalTripMiles = request.TotalTripMiles,
            PricePerMile = request.PricePerMile,
            AssignedDispatcherId = dispatcher.Id,
            AssignedTruckId = truck.Id,
        };

        await cargoRepository.AddAsync(cargoEntity);
        await cargoRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(CreateCargoCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.AssignedDispatcherId))
        {
            errorDescription = "AssignedDispatcherId is an empty string";
        }
        else if (string.IsNullOrEmpty(request.AssignedTruckId))
        {
            errorDescription = "AssignedTruckId is an empty string";
        }
        else if (string.IsNullOrEmpty(request.Source))
        {
            errorDescription = "Source address is an empty string";
        }
        else if (string.IsNullOrEmpty(request.Destination))
        {
            errorDescription = "Destination address is an empty string";
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

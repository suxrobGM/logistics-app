using Logistics.Domain.ValueObjects;

namespace Logistics.Application.Handlers.Commands;

internal sealed class CreateLoadHandler : RequestHandlerBase<CreateLoadCommand, DataResult>
{
    private readonly ITenantRepository<Load> _cargoRepository;
    private readonly ITenantRepository<Truck> _truckRepository;
    private readonly ITenantRepository<Employee> _userRepository;

    public CreateLoadHandler(
        ITenantRepository<Load> cargoRepository,
        ITenantRepository<Truck> truckRepository,
        ITenantRepository<Employee> userRepository)
    {
        _cargoRepository = cargoRepository;
        _truckRepository = truckRepository;
        _userRepository = userRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        CreateLoadCommand request, CancellationToken cancellationToken)
    {
        var dispatcher = await _userRepository.GetAsync(request.AssignedDispatcherId!);

        if (dispatcher == null)
        {
            return DataResult.CreateError("Could not find the specified dispatcher");
        }

        var truck = await _truckRepository.GetAsync(request.AssignedTruckId!);
        if (truck == null)
        {
            return DataResult.CreateError("Could not find the specified truck driver");
        }

        var cargoEntity = new Load
        {
            Name = request.Name,
            Source = request.Source,
            Status = LoadStatus.Ready,
            Destination = request.Destination,
            TotalTripMiles = request.TotalTripMiles,
            PricePerMile = request.PricePerMile,
            AssignedDispatcherId = dispatcher.Id,
            AssignedTruckId = truck.Id,
        };

        await _cargoRepository.AddAsync(cargoEntity);
        await _cargoRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(CreateLoadCommand request, out string errorDescription)
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

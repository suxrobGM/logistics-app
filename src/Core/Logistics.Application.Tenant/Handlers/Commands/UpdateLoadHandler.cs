namespace Logistics.Application.Handlers.Commands;

internal sealed class UpdateLoadHandler : RequestHandlerBase<UpdateLoadCommand, DataResult>
{
    private readonly ITenantRepository<Load> _cargoRepository;
    private readonly ITenantRepository<Truck> _truckRepository;

    public UpdateLoadHandler(
        ITenantRepository<Load> cargoRepository,
        ITenantRepository<Truck> truckRepository)
    {
        _cargoRepository = cargoRepository;
        _truckRepository = truckRepository;
    }

    protected override async Task<DataResult> HandleValidated(
        UpdateLoadCommand request, CancellationToken cancellationToken)
    {
        var truck = await _truckRepository.GetAsync(request.AssignedTruckId!);

        if (truck == null)
        {
            return DataResult.CreateError("Could not find the specified truck");
        }

        var cargoEntity = await _cargoRepository.GetAsync(request.Id!);

        if (cargoEntity == null)
        {
            return DataResult.CreateError("Could not find the specified cargo");
        }

        cargoEntity.Name = request.Name;
        cargoEntity.SourceAddress = request.SourceAddress;
        cargoEntity.DestinationAddress = request.DestinationAddress;
        cargoEntity.Distance = request.Distance;
        cargoEntity.DeliveryCost = request.DeliveryCost;
        cargoEntity.PickUpDate = request.PickUpDate;
        cargoEntity.Status = LoadStatus.Get(request.Status!);
        cargoEntity.AssignedTruck = truck;

        _cargoRepository.Update(cargoEntity);
        await _cargoRepository.UnitOfWork.CommitAsync();
        return DataResult.CreateSuccess();
    }

    protected override bool Validate(UpdateLoadCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }
        else if (string.IsNullOrEmpty(request.SourceAddress))
        {
            errorDescription = "Source address is an empty string";
        }
        else if (string.IsNullOrEmpty(request.DestinationAddress))
        {
            errorDescription = "Destination address is an empty string";
        }
        else if (string.IsNullOrEmpty(request.Status))
        {
            errorDescription = "Cargo status is not specified";
        }
        else if (string.IsNullOrEmpty(request.AssignedTruckId))
        {
            errorDescription = "AssignedTruckId is an empty string";
        }
        else if (request.DeliveryCost < 0)
        {
            errorDescription = "Price per mile should be non-negative value";
        }
        else if (request.Distance < 0)
        {
            errorDescription = "Total trip miles should be non-negative value";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}

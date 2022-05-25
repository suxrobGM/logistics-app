namespace Logistics.Application.Handlers.Queries;

internal sealed class GetCargoByIdQueryHandler : RequestHandlerBase<GetCargoByIdQuery, DataResult<CargoDto>>
{
    private readonly ITenantRepository<Cargo> _cargoRepository;

    public GetCargoByIdQueryHandler(ITenantRepository<Cargo> cargoRepository)
    {
        _cargoRepository = cargoRepository;
    }

    protected override async Task<DataResult<CargoDto>> HandleValidated(GetCargoByIdQuery request, CancellationToken cancellationToken)
    {
        var cargoEntity = await _cargoRepository.GetAsync(request.Id!);

        if (cargoEntity == null)
        {
            return DataResult<CargoDto>.CreateError("Could not find the specified cargo");
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var cargo = new CargoDto
        {
            Id = cargoEntity.Id,
            Source = cargoEntity.Source,
            Destination = cargoEntity.Destination,
            PickUpDate = cargoEntity.PickUpDate,
            PricePerMile = cargoEntity.PricePerMile,
            TotalTripMiles = cargoEntity.TotalTripMiles,
            IsCompleted = cargoEntity.IsCompleted,
            Status = cargoEntity.Status.ToString(),
            AssignedDispatcherId = cargoEntity.AssignedDispatcherId,
            AssignedDispatcherName = cargoEntity.AssignedDispatcher.GetFullName(),
            AssignedTruckId = cargoEntity.AssignedTruck.Id,
            AssignedTruckDriverName = cargoEntity.AssignedTruck.Driver.GetFullName()
        };
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        return DataResult<CargoDto>.CreateSuccess(cargo);
    }

    protected override bool Validate(GetCargoByIdQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}

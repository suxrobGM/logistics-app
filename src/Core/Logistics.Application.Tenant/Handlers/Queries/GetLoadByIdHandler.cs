namespace Logistics.Application.Handlers.Queries;

internal sealed class GetLoadByIdHandler : RequestHandlerBase<GetLoadByIdQuery, DataResult<LoadDto>>
{
    private readonly ITenantRepository<Load> _cargoRepository;

    public GetLoadByIdHandler(ITenantRepository<Load> cargoRepository)
    {
        _cargoRepository = cargoRepository;
    }

    protected override async Task<DataResult<LoadDto>> HandleValidated(GetLoadByIdQuery request, CancellationToken cancellationToken)
    {
        var cargoEntity = await _cargoRepository.GetAsync(request.Id!);

        if (cargoEntity == null)
        {
            return DataResult<LoadDto>.CreateError("Could not find the specified cargo");
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var cargo = new LoadDto
        {
            Id = cargoEntity.Id,
            Name = cargoEntity.Name,
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

        return DataResult<LoadDto>.CreateSuccess(cargo);
    }

    protected override bool Validate(GetLoadByIdQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}

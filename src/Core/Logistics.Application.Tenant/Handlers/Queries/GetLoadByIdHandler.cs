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
        var loadEntity = await _cargoRepository.GetAsync(request.Id!);

        if (loadEntity == null)
        {
            return DataResult<LoadDto>.CreateError("Could not find the specified cargo");
        }

#pragma warning disable CS8602 // Dereference of a possibly null reference.
        var load = new LoadDto
        {
            Id = loadEntity.Id,
            ReferenceId = loadEntity.ReferenceId,
            Name = loadEntity.Name,
            SourceAddress = loadEntity.SourceAddress,
            DestinationAddress = loadEntity.DestinationAddress,
            DispatchedDate = loadEntity.DispatchedDate,
            PickUpDate = loadEntity.PickUpDate,
            DeliveryDate = loadEntity.DeliveryDate,
            DeliveryCost = loadEntity.DeliveryCost,
            Distance = loadEntity.Distance,
            Status = loadEntity.Status.ToString(),
            AssignedDispatcherId = loadEntity.AssignedDispatcherId,
            AssignedDispatcherName = loadEntity.AssignedDispatcher.GetFullName(),
            AssignedTruckId = loadEntity.AssignedTruck.Id,
            AssignedTruckDriverName = loadEntity.AssignedTruck.Driver.GetFullName()
        };
#pragma warning restore CS8602 // Dereference of a possibly null reference.

        return DataResult<LoadDto>.CreateSuccess(load);
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

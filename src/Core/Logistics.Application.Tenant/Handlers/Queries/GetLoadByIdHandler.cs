namespace Logistics.Application.Handlers.Queries;

internal sealed class GetLoadByIdHandler : RequestHandlerBase<GetLoadByIdQuery, DataResult<LoadDto>>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public GetLoadByIdHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<DataResult<LoadDto>> HandleValidated(
        GetLoadByIdQuery request, CancellationToken cancellationToken)
    {
        var loadEntity = await _tenantRepository.GetAsync<Load>(request.Id);

        if (loadEntity == null)
            return DataResult<LoadDto>.CreateError("Could not find the specified cargo");

        var assignedDispatcher = await _mainRepository.GetAsync<User>(loadEntity.AssignedDispatcherId);
        var assignedDriver = await _mainRepository.GetAsync<User>(i => loadEntity.AssignedTruck != null && i.Id == loadEntity.AssignedTruck.DriverId);
        
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
            AssignedDispatcherName = assignedDispatcher?.GetFullName(),
            AssignedDriverId = assignedDriver?.Id,
            AssignedDriverName = assignedDriver?.GetFullName(),
            AssignedTruckId = loadEntity.AssignedTruckId
        };
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

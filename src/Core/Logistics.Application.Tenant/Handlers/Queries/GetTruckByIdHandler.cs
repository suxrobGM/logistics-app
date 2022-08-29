namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTruckByIdHandler : RequestHandlerBase<GetTruckByIdQuery, DataResult<TruckDto>>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public GetTruckByIdHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<DataResult<TruckDto>> HandleValidated(
        GetTruckByIdQuery request, CancellationToken cancellationToken)
    {
        var truckEntity = await _tenantRepository.GetAsync<Truck>(request.Id);

        var loadsIds = _tenantRepository.GetQuery<Truck>()
            .SelectMany(i => i.Loads)
            .Select(i => i.Id)
            .ToList();

        if (truckEntity == null)
            return DataResult<TruckDto>.CreateError("Could not find the specified truck");

        var truckDriver = await _mainRepository.GetAsync<User>(i => i.Id == truckEntity.DriverId);
        
        var truck = new TruckDto
        {
            Id = truckEntity.Id,
            TruckNumber = truckEntity.TruckNumber,
            DriverId = truckEntity.DriverId,
            DriverName = truckDriver?.GetFullName(),
            LoadIds = loadsIds
        };

        return DataResult<TruckDto>.CreateSuccess(truck);
    }

    protected override bool Validate(GetTruckByIdQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.Id))
        {
            errorDescription = "Id is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}

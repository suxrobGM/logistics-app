namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTruckByDriverHandler : RequestHandlerBase<GetTruckByDriverQuery, ResponseResult<TruckDto>>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public GetTruckByDriverHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<TruckDto>> HandleValidated(
        GetTruckByDriverQuery request, CancellationToken cancellationToken)
    {
        var truckEntity = await _tenantRepository.GetAsync<Truck>(i => i.DriverId == request.DriverId);
        var loadIds = new List<string>();

        if (request.IncludeLoadIds)
        {
            loadIds = _tenantRepository.Query<Truck>()
                .SelectMany(i => i.Loads)
                .Select(i => i.Id)
                .ToList();
        }

        if (truckEntity == null)
            return ResponseResult<TruckDto>.CreateError("Could not find the specified truck");

        var truckDriver = await _mainRepository.GetAsync<User>(i => i.Id == truckEntity.DriverId);
        
        var truck = new TruckDto
        {
            Id = truckEntity.Id,
            TruckNumber = truckEntity.TruckNumber,
            DriverId = truckEntity.DriverId,
            DriverName = truckDriver?.GetFullName(),
            LoadIds = loadIds
        };

        return ResponseResult<TruckDto>.CreateSuccess(truck);
    }

    protected override bool Validate(GetTruckByDriverQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(request.DriverId))
        {
            errorDescription = "Driver ID is an empty string";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}

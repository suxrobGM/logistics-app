using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTruckByIdHandler : RequestHandler<GetTruckByIdQuery, ResponseResult<TruckDto>>
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

    protected override async Task<ResponseResult<TruckDto>> HandleValidated(
        GetTruckByIdQuery request, CancellationToken cancellationToken)
    {
        var truckEntity = await _tenantRepository.GetAsync<Truck>(request.Id);
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

using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTruckByIdHandler : RequestHandler<GetTruckByIdQuery, ResponseResult<TruckDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTruckByIdHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<TruckDto>> HandleValidated(
        GetTruckByIdQuery req, CancellationToken cancellationToken)
    {
        var truckEntity = await _tenantRepository.GetAsync<Truck>(req.Id);
        string[]? loadIds = null;

        if (req.IncludeLoadIds)
        {
            loadIds = _tenantRepository.Query<Truck>()
                .SelectMany(i => i.Loads)
                .Select(i => i.Id)
                .ToArray();
        }

        if (truckEntity == null)
            return ResponseResult<TruckDto>.CreateError("Could not find the specified truck");
        
        var truck = new TruckDto
        {
            Id = truckEntity.Id,
            TruckNumber = truckEntity.TruckNumber,
            DriverIds = truckEntity.Drivers.Select(i => i.Id).ToArray(),
            // DriverName = truckDriver?.GetFullName(),
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

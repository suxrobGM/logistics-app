using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTruckByDriverHandler : RequestHandler<GetTruckByDriverQuery, ResponseResult<TruckDto>>
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
        GetTruckByDriverQuery req, CancellationToken cancellationToken)
    {
        string[]? loadIds = null;
        var driver = await _tenantRepository.GetAsync<Employee>(req.DriverId);
        
        if (driver == null)
            return ResponseResult<TruckDto>.CreateError($"Could not find the specified driver with ID {req.DriverId}");

        if (driver.Truck == null)
            return ResponseResult<TruckDto>.CreateError("The driver is not associated with any truck");
        
        
        if (req.IncludeLoadIds)
        {
            loadIds = _tenantRepository.Query<Truck>()
                .SelectMany(i => i.Loads)
                .Select(i => i.Id)
                .ToArray();
        }
        
        // var user = await _mainRepository.GetAsync<User>(i => i.Id == req.DriverId);
        
        var truck = new TruckDto
        {
            Id = driver.Truck.Id,
            TruckNumber = driver.Truck.TruckNumber,
            DriverIds = driver.Truck.Drivers.Select(i => i.Id).ToArray(),
            // DriverName = truckDriver?.GetFullName(),
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

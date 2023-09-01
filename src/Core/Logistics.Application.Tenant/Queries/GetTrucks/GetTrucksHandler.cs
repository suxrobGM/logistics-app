using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTrucksHandler : RequestHandler<GetTrucksQuery, PagedResponseResult<TruckDto>>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public GetTrucksHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }

    protected override async Task<PagedResponseResult<TruckDto>> HandleValidated(
        GetTrucksQuery req,
        CancellationToken cancellationToken)
    {
        var loadIds = new List<string>();
        if (req.IncludeLoadIds)
        {
            loadIds = _tenantRepository.Query<Truck>()
                .SelectMany(i => i.Loads)
                .Select(i => i.Id)
                .ToList();
        }

        var tenant = await _tenantRepository.GetCurrentTenantAsync();
        var totalItems = _tenantRepository.Query<Truck>().Count();
        var filteredUsers = _mainRepository.ApplySpecification(new FilterUsersByTenantId(tenant.Id)).ToArray();
        var userIds = filteredUsers.Select(i => i.Id).ToArray();
        var userNames = filteredUsers.Select(i => i.UserName).ToArray();
        var userFirstNames = filteredUsers.Select(i => i.FirstName).ToArray();
        var userLastNames = filteredUsers.Select(i => i.LastName).ToArray();
        var spec = new SearchTrucks(req.Search, userIds, userNames!, userFirstNames, userLastNames, req.Descending);

        var trucks = _tenantRepository
            .ApplySpecification(spec)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToArray();

        var driverIds = trucks.Where(i => !string.IsNullOrEmpty(i.DriverId))
            .Select(i => i.DriverId);

        var drivers = _mainRepository.Query<User>()
            .Where(user => driverIds.Contains(user.Id))
            .ToDictionary(i => i.Id);

        var trucksDto = trucks.Select(i => new TruckDto
        {
            Id = i.Id,
            TruckNumber = i.TruckNumber,
            DriverId = i.DriverId,
            LoadIds = loadIds
        }).ToArray();

        foreach (var truck in trucksDto)
        {
            var driverId = truck.DriverId;

            if (!string.IsNullOrEmpty(driverId) && drivers.TryGetValue(driverId, out var user))
            {
                truck.DriverName = user.GetFullName();
            }
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return new PagedResponseResult<TruckDto>(trucksDto, totalItems, totalPages);
    }

    protected override bool Validate(GetTrucksQuery query, out string errorDescription)
    {
        errorDescription = string.Empty;

        // if (string.IsNullOrEmpty(_tenantRepository.CurrentTenant?.Id))
        // {
        //     errorDescription = "Could not evaluate current tenant's ID";
        // }
        if (query.Page <= 0)
        {
            errorDescription = "Page number should be non-negative";
        }
        else if (query.PageSize <= 1)
        {
            errorDescription = "Page size should be greater than one";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
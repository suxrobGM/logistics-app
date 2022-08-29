namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTrucksHandler : RequestHandlerBase<GetTrucksQuery, PagedDataResult<TruckDto>>
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

    protected override Task<PagedDataResult<TruckDto>> HandleValidated(
        GetTrucksQuery request,
        CancellationToken cancellationToken)
    {
        var loadIds = new List<string>();
        if (request.IncludeLoadIds)
        {
            loadIds = _tenantRepository.GetQuery<Truck>()
                .SelectMany(i => i.Loads)
                .Select(i => i.Id)
                .ToList();
        }

        var tenantId = _tenantRepository.CurrentTenant!.Id;
        var totalItems = _tenantRepository.GetQuery<Truck>().Count();
        var filteredUsers = _mainRepository.ApplySpecification(new FilterUsersByTenantId(tenantId)).ToArray();
        var userIds = filteredUsers.Select(i => i.Id).ToArray();
        var userNames = filteredUsers.Select(i => i.UserName).ToArray();
        var userFirstNames = filteredUsers.Select(i => i.FirstName).ToArray();
        var userLastNames = filteredUsers.Select(i => i.LastName).ToArray();

        var trucks = _tenantRepository
            .ApplySpecification(new SearchTrucks(request.Search, userIds, userNames, userFirstNames, userLastNames))
            .OrderBy(i => i.TruckNumber)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArray();

        var driverIds = trucks.Where(i => !string.IsNullOrEmpty(i.DriverId))
            .Select(i => i.DriverId);

        var drivers = _mainRepository.GetQuery<User>()
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

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<TruckDto>(trucksDto, totalItems, totalPages));
    }

    protected override bool Validate(GetTrucksQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(_tenantRepository.CurrentTenant?.Id))
        {
            errorDescription = "Could not evaluate current tenant's ID";
        }
        else if (request.Page <= 0)
        {
            errorDescription = "Page number should be non-negative";
        }
        else if (request.PageSize <= 1)
        {
            errorDescription = "Page size should be greater than one";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
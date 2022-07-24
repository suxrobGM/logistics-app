namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTrucksHandler : RequestHandlerBase<GetTrucksQuery, PagedDataResult<TruckDto>>
{
    private readonly IMainRepository<User> _userRepository;
    private readonly ITenantRepository<Truck> _truckRepository;

    public GetTrucksHandler(
        IMainRepository<User> userRepository,
        ITenantRepository<Truck> truckRepository)
    {
        _userRepository = userRepository;
        _truckRepository = truckRepository;
    }

    protected override Task<PagedDataResult<TruckDto>> HandleValidated(
        GetTrucksQuery request, 
        CancellationToken cancellationToken)
    {
        var loadIds = new List<string>();
        if (request.IncludeLoadIds)
        {
            loadIds = _truckRepository.GetQuery()
                        .SelectMany(i => i.Loads)
                        .Select(i => i.Id)
                        .ToList();
        }

        var totalItems = _truckRepository.GetQuery().Count();
        var itemsQuery = _truckRepository.GetQuery();

        if (!string.IsNullOrEmpty(request.Search))
        {
            itemsQuery = _truckRepository.GetQuery(new SearchTrucksSpecification(request.Search));
        }

        var trucks = itemsQuery
                .OrderBy(i => i.Id)
                .Skip((request.Page - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToArray();
        
        var driverIds = trucks.Where(i => !string.IsNullOrEmpty(i.DriverId))
            .Select(i => i.DriverId);

        var drivers = _userRepository.GetQuery()
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

            if (!string.IsNullOrEmpty(driverId))
            {
                truck.DriverName = drivers[driverId].GetFullName();
            }
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<TruckDto>(trucksDto, totalItems, totalPages));
    }

    protected override bool Validate(GetTrucksQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (request.Page <= 0)
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

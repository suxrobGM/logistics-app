namespace Logistics.Application.Handlers.Queries;

internal sealed class GetLoadsHandler : RequestHandlerBase<GetLoadsQuery, PagedDataResult<LoadDto>>
{
    private readonly IMainRepository<User> _userRepository;
    private readonly ITenantRepository<Load> _loadRepository;
    private readonly ITenantRepository<Truck> _truckRepository;

    public GetLoadsHandler(
        IMainRepository<User> userRepository,
        ITenantRepository<Load> loadRepository,
        ITenantRepository<Truck> truckRepository)
    {
        _userRepository = userRepository;
        _loadRepository = loadRepository;
        _truckRepository = truckRepository;
    }

    protected override async Task<PagedDataResult<LoadDto>> HandleValidated(
        GetLoadsQuery request, 
        CancellationToken cancellationToken)
    {
        var totalItems = _loadRepository.GetQuery().Count();
        var loadsQuery = _loadRepository.GetQuery();

        if (!string.IsNullOrEmpty(request.Search))
        {
            loadsQuery = _loadRepository.GetQuery(new LoadsSpecification(request.Search));
        }

        var loads = loadsQuery
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            //.OrderBy(i => i.Id)
            .Select(i => new LoadDto
            {
                Id = i.Id,
                ReferenceId = i.ReferenceId,
                Name = i.Name,
                SourceAddress = i.SourceAddress,
                DestinationAddress = i.DestinationAddress,
                DeliveryCost = i.DeliveryCost,
                Distance = i.Distance,
                DispatchedDate = i.DispatchedDate,
                PickUpDate = i.PickUpDate,
                DeliveryDate = i.DeliveryDate,
                AssignedDispatcherId = i.AssignedDispatcherId,
                AssignedTruckId = i.AssignedTruck != null ? i.AssignedTruck.Id : null,
                Status = i.Status.Name
            })
            .ToArray();

        // TODO: need to optimize query
        foreach (var load in loads)
        {
            var assignedDispatcher = await _userRepository.GetAsync(load.AssignedDispatcherId!);
            var assignedTruck = await _truckRepository.GetAsync(load.AssignedTruckId!);
            if (assignedTruck != null)
            {
                var assignedTruckDriver = await _userRepository.GetAsync(assignedTruck.Driver?.Id!);
                load.AssignedTruckDriverName = assignedTruckDriver?.GetFullName();
            }
            
            load.AssignedDispatcherName = assignedDispatcher?.GetFullName();
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return new PagedDataResult<LoadDto>(loads, totalItems, totalPages);
    }

    protected override bool Validate(GetLoadsQuery request, out string errorDescription)
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

namespace Logistics.Application.Handlers.Queries;

internal sealed class GetLoadsHandler : RequestHandlerBase<GetLoadsQuery, PagedDataResult<LoadDto>>
{
    private readonly IMainRepository<User> _userRepository;
    private readonly ITenantRepository<Load> _loadRepository;

    public GetLoadsHandler(
        IMainRepository<User> userRepository,
        ITenantRepository<Load> loadRepository)
    {
        _userRepository = userRepository;
        _loadRepository = loadRepository;
    }

    protected override Task<PagedDataResult<LoadDto>> HandleValidated(
        GetLoadsQuery request, 
        CancellationToken cancellationToken)
    {
        var tenantId = _loadRepository.CurrentTenant!.Id;
        var totalItems = _loadRepository.GetQuery().Count();
        var loadsQuery = _loadRepository.GetQuery();
        var filteredUsers = _userRepository.GetQuery(new FilterUsersByTenantId(tenantId)).ToArray();
        var userIds = filteredUsers.Select(i => i.Id).ToArray();
        var userNames = filteredUsers.Select(i => i.UserName).ToArray();
        var userFirstNames = filteredUsers.Select(i => i.FirstName).ToArray();
        var userLastNames = filteredUsers.Select(i => i.LastName).ToArray();

        if (!string.IsNullOrEmpty(request.Search))
        {
            loadsQuery = _loadRepository.GetQuery(new SearchLoads(request.Search, userIds, userNames, userFirstNames, userLastNames));
        }

        var loads = loadsQuery
            .OrderBy(i => i.Id)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToArray();

        var driverIds = loads.Where(i => !string.IsNullOrEmpty(i.AssignedDriverId))
            .Select(i => i.AssignedDriverId);
        
        var dispatcherIds = loads.Where(i => !string.IsNullOrEmpty(i.AssignedDispatcherId))
            .Select(i => i.AssignedDispatcherId);

        var drivers = _userRepository.GetQuery()
            .Where(user => driverIds.Contains(user.Id))
            .ToDictionary(i => i.Id);
        
        var dispatchers = _userRepository.GetQuery()
            .Where(user => dispatcherIds.Contains(user.Id))
            .ToDictionary(i => i.Id);

        var loadsDto = loads.Select(i => new LoadDto
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
            AssignedDriverId = i.AssignedDriverId,
            AssignedTruckId = i.AssignedTruckId,
            Status = i.Status.Name
        }).ToArray();
        
        foreach (var loadDto in loadsDto)
        {
            var dispatcherId = loadDto.AssignedDispatcherId;
            var driverId = loadDto.AssignedDriverId;
            
            if (!string.IsNullOrWhiteSpace(dispatcherId))
                loadDto.AssignedDispatcherName = dispatchers[dispatcherId].GetFullName();
            
            if (!string.IsNullOrWhiteSpace(driverId))
                loadDto.AssignedDriverName = drivers[driverId].GetFullName();
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)request.PageSize);
        return Task.FromResult(new PagedDataResult<LoadDto>(loadsDto, totalItems, totalPages));
    }

    protected override bool Validate(GetLoadsQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (string.IsNullOrEmpty(_loadRepository.CurrentTenant?.Id))
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

namespace Logistics.Application.Tenant.Handlers.Queries;

internal sealed class GetLoadsHandler : RequestHandlerBase<GetLoadsRequest, PagedResponseResult<LoadDto>>
{
    private readonly IMainRepository _mainRepository;
    private readonly ITenantRepository _tenantRepository;

    public GetLoadsHandler(
        IMainRepository mainRepository,
        ITenantRepository tenantRepository)
    {
        _mainRepository = mainRepository;
        _tenantRepository = tenantRepository;
    }

    protected override Task<PagedResponseResult<LoadDto>> HandleValidated(
        GetLoadsRequest req, 
        CancellationToken cancellationToken)
    {
        var tenantId = _tenantRepository.CurrentTenant!.Id;
        var totalItems = _tenantRepository.Query<Load>().Count();
        var filteredUsers = _mainRepository.ApplySpecification(new FilterUsersByTenantId(tenantId)).ToArray();
        var userIds = filteredUsers.Select(i => i.Id).ToArray();
        var userNames = filteredUsers.Select(i => i.UserName).ToArray();
        var userFirstNames = filteredUsers.Select(i => i.FirstName).ToArray();
        var userLastNames = filteredUsers.Select(i => i.LastName).ToArray();
        var spec = new SearchLoads(req.Search, userIds, userNames!, userFirstNames, userLastNames, req.OrderBy, req.Descending);

        var loads = _tenantRepository
            .ApplySpecification(spec)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToArray();

        var driverIds = loads.Where(i => !string.IsNullOrEmpty(i.AssignedDriverId))
            .Select(i => i.AssignedDriverId);
        
        var dispatcherIds = loads.Where(i => !string.IsNullOrEmpty(i.AssignedDispatcherId))
            .Select(i => i.AssignedDispatcherId);

        var drivers = _mainRepository.Query<User>()
            .Where(user => driverIds.Contains(user.Id))
            .ToDictionary(i => i.Id);
        
        var dispatchers = _mainRepository.Query<User>()
            .Where(user => dispatcherIds.Contains(user.Id))
            .ToDictionary(i => i.Id);

        var loadsDto = loads.Select(i => new LoadDto
        {
            Id = i.Id,
            RefId = i.RefId,
            Name = i.Name,
            SourceAddress = i.SourceAddress,
            DestinationAddress = i.DestinationAddress,
            DeliveryCost = i.DeliveryCost,
            Distance = i.Distance,
            DispatchedDate = i.DispatchedDate,
            PickUpDate = i.PickUpDate,
            DeliveryDate = i.DeliveryDate,
            Status = i.Status,
            AssignedDispatcherId = i.AssignedDispatcherId,
            AssignedDriverId = i.AssignedDriverId,
            AssignedTruckId = i.AssignedTruckId
        }).ToArray();
        
        foreach (var loadDto in loadsDto)
        {
            var dispatcherId = loadDto.AssignedDispatcherId;
            var driverId = loadDto.AssignedDriverId;

            if (!string.IsNullOrWhiteSpace(dispatcherId) &&
                dispatchers.TryGetValue(dispatcherId, out var dispatcher))
            {
                loadDto.AssignedDispatcherName = dispatcher.GetFullName();
            }
            
            if (!string.IsNullOrWhiteSpace(driverId) && 
                drivers.TryGetValue(driverId, out var driver))
            {
                loadDto.AssignedDriverName = driver.GetFullName();
            }
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(new PagedResponseResult<LoadDto>(loadsDto, totalItems, totalPages));
    }

    protected override bool Validate(GetLoadsRequest request, out string errorDescription)
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

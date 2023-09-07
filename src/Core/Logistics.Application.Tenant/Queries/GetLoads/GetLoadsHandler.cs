using Logistics.Application.Tenant.Mappers;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetLoadsHandler : RequestHandler<GetLoadsQuery, PagedResponseResult<LoadDto>>
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
        GetLoadsQuery req, 
        CancellationToken cancellationToken)
    {
        var tenant = _tenantRepository.GetCurrentTenant();
        var totalItems = _tenantRepository.Query<Load>().Count();
        var filteredUsers = _mainRepository.ApplySpecification(new FilterUsersByTenantId(tenant.Id)).ToArray();
        var userIds = filteredUsers.Select(i => i.Id).ToArray();
        var userFirstNames = filteredUsers.Select(i => i.FirstName).ToArray();
        var userLastNames = filteredUsers.Select(i => i.LastName).ToArray();
        var spec = new SearchLoads(req.Search, userIds, userFirstNames, userLastNames, req.OrderBy, req.Descending);

        var loads = _tenantRepository
            .ApplySpecification(spec)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .ToArray();

        // var driverIds = loads.Where(i => !string.IsNullOrEmpty(i.AssignedDriverId))
        //     .Select(i => i.AssignedDriverId);
        
        var dispatcherIds = loads.Where(i => !string.IsNullOrEmpty(i.AssignedDispatcherId))
            .Select(i => i.AssignedDispatcherId);

        // var drivers = _mainRepository.Query<User>()
        //     .Where(user => driverIds.Contains(user.Id))
        //     .ToDictionary(i => i.Id);
        
        var dispatchers = _mainRepository.Query<User>()
            .Where(user => dispatcherIds.Contains(user.Id))
            .ToDictionary(i => i.Id);

        var loadsDto = loads.Select(LoadMapper.ToDto).ToArray();
        
        foreach (var loadDto in loadsDto)
        {
            var dispatcherId = loadDto!.AssignedDispatcherId;

            if (!string.IsNullOrWhiteSpace(dispatcherId) &&
                dispatchers.TryGetValue(dispatcherId, out var dispatcher))
            {
                loadDto.AssignedDispatcherName = dispatcher.GetFullName();
            }
        }

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(new PagedResponseResult<LoadDto>(loadsDto!, totalItems, totalPages));
    }

    protected override bool Validate(GetLoadsQuery query, out string errorDescription)
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

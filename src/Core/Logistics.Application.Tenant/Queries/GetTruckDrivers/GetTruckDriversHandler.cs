using Logistics.Application.Tenant.Mappers;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetTruckDriversHandler : RequestHandler<GetTruckDriversQuery, PagedResponseResult<TruckDriversDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTruckDriversHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override Task<PagedResponseResult<TruckDriversDto>> HandleValidated(
        GetTruckDriversQuery req, CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<Employee>().Count();
        var drivers = _tenantRepository.Query<Employee>().Where(i => !string.IsNullOrEmpty(i.TruckId));

        var truckDriversQuery = from truck in _tenantRepository.Query<Truck>()
                join driver in drivers on truck.Id equals driver.TruckId into teamDrivers
                from teamDriver in teamDrivers.DefaultIfEmpty()
                where teamDriver != null &&
                      teamDriver.FirstName!.Contains(req.Search!) ||
                      teamDriver.LastName!.Contains(req.Search!) ||
                      teamDriver.Email!.Contains(req.Search!) ||
                      truck.TruckNumber!.Contains(req.Search!)
                select new
                {
                    Truck = truck,
                    TeamDriver = teamDriver
                };

        var truckDriversDto = truckDriversQuery
            .GroupBy(td => td.Truck)
            .Select(g => new TruckDriversDto
            {
                Truck = g.Key.ToDto(),
                Drivers = g.Where(td => td.TeamDriver != null)
                    .Select(td => td.TeamDriver.ToDto())
            });

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(new PagedResponseResult<TruckDriversDto>(truckDriversDto, totalItems, totalPages));
    }
}

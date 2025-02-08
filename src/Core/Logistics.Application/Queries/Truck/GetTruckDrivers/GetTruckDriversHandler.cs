using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetTruckDriversHandler : RequestHandler<GetTruckDriversQuery, PagedResult<TruckDriversDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetTruckDriversHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<PagedResult<TruckDriversDto>> HandleValidated(
        GetTruckDriversQuery req, CancellationToken cancellationToken)
    {
        var employeeRepository = _tenantUow.Repository<Employee>();
        var truckRepository = _tenantUow.Repository<Truck>();
        var totalItems = await employeeRepository.CountAsync();
        var drivers = employeeRepository.ApplySpecification(new GetDrivers());

        var truckDriversQuery = from truck in truckRepository.Query()
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
            .ToArray()
            .Select(g => new TruckDriversDto
            {
                Truck = g.Key.ToDto(new List<LoadDto>()),
                Drivers = g.Where(td => td.TeamDriver != null)
                    .Select(td => td.TeamDriver.ToDto())
            });
        
        return PagedResult<TruckDriversDto>.Succeed(truckDriversDto, totalItems, req.PageSize);
    }
}

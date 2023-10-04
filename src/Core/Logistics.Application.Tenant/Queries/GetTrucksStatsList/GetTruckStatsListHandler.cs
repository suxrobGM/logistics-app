using System.Linq.Expressions;
using Logistics.Application.Tenant.Mappers;
using Logistics.Domain.Enums;
using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

public class GetTruckStatsListHandler : RequestHandler<GetTrucksStatsListQuery, PagedResponseResult<TruckStatsDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTruckStatsListHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    protected override Task<PagedResponseResult<TruckStatsDto>> HandleValidated(
        GetTrucksStatsListQuery req, CancellationToken cancellationToken)
    {
        var totalItems = _tenantRepository.Query<Truck>().Count();

        var truckStatsQuery = _tenantRepository.Query<Load>()
            .Where(load => load.DeliveryDate.HasValue
                           && load.DeliveryDate >= req.StartDate
                           && load.DeliveryDate.Value <= req.EndDate)
            .GroupBy(load => load.AssignedTruckId!)
            .Select(group => new TruckStats
            {
                TruckId = group.Key,
                TruckNumber = group.First().AssignedTruck!.TruckNumber!,
                Gross = group.Sum(load => load.DeliveryCost),
                Distance = group.Sum(load => load.Distance),
                DriverShare = group.Sum(load => load.DeliveryCost) * (decimal)group.First().AssignedTruck!.DriverIncomePercentage,
                FirstLoad = group.FirstOrDefault()
            });

        truckStatsQuery = req.Descending
            ? truckStatsQuery.OrderByDescending(InitOrderBy(req.OrderBy))
            : truckStatsQuery.OrderBy(InitOrderBy(req.OrderBy));

        truckStatsQuery = truckStatsQuery
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize);
            
        var truckStatsDto = truckStatsQuery.ToArray()
            .Select(result => new TruckStatsDto
            {
                TruckId = result.TruckId,
                TruckNumber = result.TruckNumber,
                StartDate = req.StartDate,
                EndDate = req.EndDate,
                Gross = result.Gross,
                Distance = result.Distance,
                DriverShare = result.DriverShare, 
                Drivers = result.FirstLoad?.AssignedTruck?.Drivers.Select(driver => driver.ToDto()) 
                          ?? new List<EmployeeDto>()
            });    

        var totalPages = (int)Math.Ceiling(totalItems / (double)req.PageSize);
        return Task.FromResult(new PagedResponseResult<TruckStatsDto>(truckStatsDto, totalItems, totalPages));
    }
    
    private static Expression<Func<TruckStats, object>> InitOrderBy(string? propertyName)
    {
        propertyName = propertyName?.ToLower() ?? string.Empty;
        return propertyName switch
        {
            "distance" => i => i.Distance,
            "gross" => i => i.Gross,
            "drivershare" => i => i.DriverShare,
            _ => i => i.TruckNumber
        };
    }

    private record TruckStats
    {
        public required string TruckId { get; set; }
        public required string TruckNumber { get; set; }
        public required decimal Gross { get; set; }
        public required double Distance { get; set; }
        public required decimal DriverShare { get; set; }
        public Load? FirstLoad { get; set; }
    }
}

using System.Linq.Expressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetTruckStatsListHandler : RequestHandler<GetTrucksStatsListQuery, PagedResult<TruckStatsDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetTruckStatsListHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }
    
    protected override async Task<PagedResult<TruckStatsDto>> HandleValidated(
        GetTrucksStatsListQuery req, CancellationToken cancellationToken)
    {
        var orderBy = req.OrderBy ?? string.Empty;
        var isDescendingOrder = false;
        
        if (orderBy.StartsWith('-'))
        {
            orderBy = orderBy[1..];
            isDescendingOrder = true;
        }
        
        var totalItems = await _tenantUow.Repository<Truck>().CountAsync();
        var truckStatsQuery = _tenantUow.Repository<Load>()
            .ApplySpecification(new FilterLoadsByDeliveryDate(null, req.StartDate, req.EndDate))
            .GroupBy(load => load.AssignedTruckId!)
            .Select(group => new TruckStats
            {
                TruckId = group.Key,
                TruckNumber = group.First().AssignedTruck!.TruckNumber,
                Gross = group.Sum(load => load.DeliveryCost),
                Distance = group.Sum(load => load.Distance),
                DriverShare = group.Sum(load => load.DeliveryCost) * (decimal)group.First().AssignedTruck!.GetDriversShareRatio(),
                Drivers = group.First().AssignedTruck!.Drivers,
            });

        
        truckStatsQuery = truckStatsQuery
            .OrderBy(InitOrderBy(orderBy), isDescendingOrder)
            .ApplyPaging(req.Page, req.PageSize);

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
                Drivers = result.Drivers.Select(i => i.ToDto())
            });
        
        return PagedResult<TruckStatsDto>.Succeed(truckStatsDto, totalItems, req.PageSize);
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
        public IEnumerable<Employee> Drivers { get; set; } = Array.Empty<Employee>();
    }
}

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
        IEnumerable<TruckStatsDto> truckStatsDto = new List<TruckStatsDto>();
        var totalItems = 0;
        try
        {
            FormattableString query = $@"
                SELECT * FROM get_trucks_stats(
                    {req.StartDate}::timestamp,
                    {req.EndDate}::timestamp,
                    {req.Page},
                    {req.PageSize},
                    {req.OrderBy}
                )";
            truckStatsDto = await _tenantUow.ExecuteRawSql<TruckStatsDto>(query);
            totalItems = truckStatsDto.FirstOrDefault()?.TotalItems ?? 0;
        }
        catch
        {
            var orderBy = req.OrderBy ?? string.Empty;
            var isDescendingOrder = false;

            if (orderBy.StartsWith('-'))
            {
                orderBy = orderBy[1..];
                isDescendingOrder = true;
            }

            totalItems = await _tenantUow.Repository<Truck>().CountAsync();
            var truckStatsQuery = _tenantUow.Repository<Load>()
                .ApplySpecification(new FilterLoadsByDeliveryDate(null, req.StartDate, req.EndDate))
                .GroupBy(load => load.AssignedTruckId!)
                .Select(group => new TruckStats
                {
                    TruckId = group.Key!.Value,
                    TruckNumber = group.First().AssignedTruck!.TruckNumber,
                    Gross = group.Sum(load => load.DeliveryCost),
                    Distance = group.Sum(load => load.Distance),
                    DriverShare = group.Sum(load => load.DeliveryCost) * (decimal)group.First().AssignedTruck!.GetDriversShareRatio(),
                    Drivers = group.First().AssignedTruck!.Drivers,
                });


            truckStatsQuery = truckStatsQuery
                .OrderBy(InitOrderBy(orderBy), isDescendingOrder)
                .ApplyPaging(req.Page, req.PageSize);

            truckStatsDto = truckStatsQuery.ToArray()
                .Select(result => new TruckStatsDto
                {
                    TruckId = result.TruckId,
                    TruckNumber = result.TruckNumber,
                    StartDate = req.StartDate,
                    EndDate = req.EndDate,
                    Gross = result.Gross,
                    Distance = result.Distance,
                    DriverShare = result.DriverShare,
                    Drivers = result.Drivers.Select(i => i.ToDto()).ToList()
                });
        }

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
        public required Guid TruckId { get; set; }
        public required string TruckNumber { get; set; }
        public required decimal Gross { get; set; }
        public required double Distance { get; set; }
        public required decimal DriverShare { get; set; }
        public IEnumerable<Employee> Drivers { get; set; } = [];
    }
}

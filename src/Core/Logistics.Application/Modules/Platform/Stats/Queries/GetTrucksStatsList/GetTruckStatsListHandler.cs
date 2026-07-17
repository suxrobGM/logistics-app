using Logistics.Application.Abstractions;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Platform.Stats.Queries;

public class GetTruckStatsListHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTrucksStatsListQuery, PagedResult<TruckStatsDto>>
{
    public async Task<PagedResult<TruckStatsDto>> Handle(
        GetTrucksStatsListQuery req, CancellationToken ct)
    {
        var query = $"""
            SELECT * FROM get_trucks_stats(
                '{req.StartDate:yyyy-MM-dd HH:mm:ss}'::timestamp,
                '{req.EndDate:yyyy-MM-dd HH:mm:ss}'::timestamp,
                {req.Page},
                {req.PageSize},
                '{req.OrderBy}'
            );
        """;
        var truckStatsDto = await tenantUow.ExecuteRawSql<TruckStatsDto>(query);
        var totalItems = truckStatsDto.FirstOrDefault()?.TotalItems ?? 0;

        return PagedResult<TruckStatsDto>.Ok(truckStatsDto, totalItems, req.PageSize);
    }
}

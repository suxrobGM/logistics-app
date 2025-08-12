using Logistics.Application.Abstractions;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

public class GetTruckStatsListHandler : IAppRequestHandler<GetTrucksStatsListQuery, PagedResult<TruckStatsDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetTruckStatsListHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public async Task<PagedResult<TruckStatsDto>> Handle(
        GetTrucksStatsListQuery req, CancellationToken ct)
    {
        var query = $"""
                     SELECT * FROM get_trucks_stats(
                         {req.StartDate}::timestamp,
                         {req.EndDate}::timestamp,
                         {req.Page},
                         {req.PageSize},
                         {req.OrderBy}
                     )
                     """;
        var truckStatsDto = await _tenantUow.ExecuteRawSql<TruckStatsDto>(query);
        var totalItems = truckStatsDto.FirstOrDefault()?.TotalItems ?? 0;

        return PagedResult<TruckStatsDto>.Succeed(truckStatsDto, totalItems, req.PageSize);
    }
}

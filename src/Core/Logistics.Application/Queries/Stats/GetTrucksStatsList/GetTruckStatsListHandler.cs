using Logistics.Domain.Persistence;
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

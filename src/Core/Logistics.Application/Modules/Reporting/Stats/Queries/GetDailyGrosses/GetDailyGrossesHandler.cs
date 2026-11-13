using Logistics.Application.Abstractions;
using Logistics.Application.Modules.Operations.Loads;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Reporting.Stats.Queries;

internal sealed class GetDailyGrossesHandler(ITenantUnitOfWork tenantUow) : IRequestHandler<GetDailyGrossesQuery, Result<DailyGrossesDto>>
{
    public async Task<Result<DailyGrossesDto>> Handle(
        GetDailyGrossesQuery req, CancellationToken ct)
    {
        var truckId = req.TruckId;

        if (req.UserId.HasValue)
        {
            var truck = await tenantUow.Repository<Truck>().GetAsync(i => i.MainDriverId == req.UserId.Value ||
                                                                           i.SecondaryDriverId == req.UserId.Value, ct);

            if (truck is null)
            {
                return Result<DailyGrossesDto>.Fail($"Could not find a truck with driver ID '{req.UserId}'", ErrorCodes.NotFound);
            }

            truckId = truck.Id;
        }

        // Ensure dates are UTC for PostgreSQL compatibility
        var startDate = DateTime.SpecifyKind(req.StartDate, DateTimeKind.Utc);
        var endDate = DateTime.SpecifyKind(req.EndDate, DateTimeKind.Utc);

        var days = req.StartDate.DaysBetween(req.EndDate);
        var dict = days.ToDictionary(
            k => (k.Year, k.Month, k.Day),
            m => new DailyGrossDto(m.Year, m.Month, m.Day));

        var filteredLoads = await tenantUow.Repository<Load>().Query().FilterDeliveredBetween(truckId, startDate, endDate).ToArrayAsync(ct);

        foreach (var load in filteredLoads)
        {
            var date = load.DeliveredAt!.Value;
            var key = (date.Year, date.Month, date.Day);

            if (!dict.ContainsKey(key))
            {
                continue;
            }

            dict[key].Gross += load.DeliveryCost;
            dict[key].Distance += load.Distance;
            dict[key].DriverShare += load.CalcDriverShare();
        }

        var dailyGrosses = new DailyGrossesDto
        {
            Data = dict.Values
        };
        return Result<DailyGrossesDto>.Ok(dailyGrosses);
    }
}

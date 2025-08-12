using Logistics.Application.Abstractions;
using Logistics.Application.Specifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Extensions;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetMonthlyGrossesHandler : RequestHandler<GetMonthlyGrossesQuery, Result<MonthlyGrossesDto>>
{
    private readonly ITenantUnitOfWork _tenantUow;

    public GetMonthlyGrossesHandler(ITenantUnitOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    public override async Task<Result<MonthlyGrossesDto>> Handle(
        GetMonthlyGrossesQuery req, CancellationToken ct)
    {
        var truckId = req.TruckId;

        if (req.UserId.HasValue)
        {
            var truck = await _tenantUow.Repository<Truck>().GetAsync(i => i.MainDriverId == req.UserId.Value ||
                                                                           i.SecondaryDriverId == req.UserId.Value);

            if (truck is null)
            {
                return Result<MonthlyGrossesDto>.Fail($"Could not find a truck with driver ID '{req.UserId}'");
            }

            truckId = truck.Id;
        }

        var spec = new FilterLoadsByDeliveryDate(truckId, req.StartDate, req.EndDate);
        var months = req.StartDate.MonthsBetween(req.EndDate);
        var filteredLoads = _tenantUow.Repository<Load>().ApplySpecification(spec).ToArray();

        var dict = months.ToDictionary(
            k => (k.Year, k.Month),
            m => new MonthlyGrossDto(m.Year, m.Month));

        foreach (var load in filteredLoads)
        {
            var date = load.DeliveryDate!.Value;
            var key = (date.Year, date.Month);

            if (!dict.ContainsKey(key))
            {
                continue;
            }

            dict[key].Distance += load.Distance;
            dict[key].Gross += load.DeliveryCost;
            dict[key].DriverShare += load.CalcDriverShare();
        }

        var monthlyGrosses = new MonthlyGrossesDto
        {
            Data = dict.Values
        };
        return Result<MonthlyGrossesDto>.Succeed(monthlyGrosses);
    }
}

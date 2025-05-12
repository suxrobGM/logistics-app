using Logistics.Domain.Entities;
using Logistics.Domain.Extensions;
using Logistics.Domain.Persistence;
using Logistics.Domain.Specifications;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetDailyGrossesHandler : RequestHandler<GetDailyGrossesQuery, Result<DailyGrossesDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetDailyGrossesHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }
    
    protected override async Task<Result<DailyGrossesDto>> HandleValidated(
        GetDailyGrossesQuery req, CancellationToken cancellationToken)
    {
        var truckId = req.TruckId;
        
        if (req.UserId.HasValue)
        {
            var driver = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UserId.Value);

            if (driver is null)
            {
                return Result<DailyGrossesDto>.Fail($"Could not find user with ID '{req.UserId}'");
            }
            
            truckId = driver.TruckId;
        }
        
        var spec = new FilterLoadsByDeliveryDate(truckId, req.StartDate, req.EndDate);
        
        var days = req.StartDate.DaysBetween(req.EndDate);
        var dict = days.ToDictionary(
            k => (k.Year, k.Month, k.Day), 
            m => new DailyGrossDto(m.Year, m.Month, m.Day));
        
        var filteredLoads = _tenantUow.Repository<Load>().ApplySpecification(spec).ToArray();

        foreach (var load in filteredLoads)
        {
            var date = load.DeliveryDate!.Value;
            var key = (date.Year, date.Month, date.Day);

            if (!dict.ContainsKey(key)) 
                continue;
            
            dict[key].Gross += load.DeliveryCost;
            dict[key].Distance += load.Distance;
            dict[key].DriverShare += load.CalcDriverShare();
        }

        var dailyGrosses = new DailyGrossesDto
        {
            Data = dict.Values
        };
        return Result<DailyGrossesDto>.Succeed(dailyGrosses);
    }
}

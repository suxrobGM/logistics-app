using Logistics.Shared.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetMonthlyGrossesHandler : RequestHandler<GetMonthlyGrossesQuery, ResponseResult<MonthlyGrossesDto>>
{
    private readonly ITenantUnityOfWork _tenantUow;

    public GetMonthlyGrossesHandler(ITenantUnityOfWork tenantUow)
    {
        _tenantUow = tenantUow;
    }

    protected override async Task<ResponseResult<MonthlyGrossesDto>> HandleValidated(
        GetMonthlyGrossesQuery req, CancellationToken cancellationToken)
    {
        var truckId = req.TruckId;
        
        if (!string.IsNullOrEmpty(req.UserId))
        {
            var driver = await _tenantUow.Repository<Employee>().GetByIdAsync(req.UserId);

            if (driver is null)
            {
                return ResponseResult<MonthlyGrossesDto>.CreateError($"Could not find user with ID '{req.UserId}'");
            }
            
            truckId = driver.TruckId;
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
                continue;
            
            dict[key].Distance += load.Distance;
            dict[key].Gross += load.DeliveryCost;
            dict[key].DriverShare += load.CalcDriverShare();
        }

        var monthlyGrosses = new MonthlyGrossesDto
        {
            Data = dict.Values
        };
        return ResponseResult<MonthlyGrossesDto>.CreateSuccess(monthlyGrosses);
    }
}

using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetDailyGrossesHandler : RequestHandler<GetDailyGrossesQuery, ResponseResult<DailyGrossesDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetDailyGrossesHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    protected override async Task<ResponseResult<DailyGrossesDto>> HandleValidated(
        GetDailyGrossesQuery req, CancellationToken cancellationToken)
    {
        var truckId = req.TruckId;
        
        if (!string.IsNullOrEmpty(req.UserId))
        {
            var driver = await _tenantRepository.GetAsync<Employee>(req.UserId);

            if (driver is null)
            {
                return ResponseResult<DailyGrossesDto>.CreateError($"Could not find user with ID '{req.UserId}'");
            }
            
            truckId = driver.TruckId;
        }
        
        var spec = new FilterLoadsByInterval(truckId, req.StartDate, req.EndDate);
        var dailyGrosses = new DailyGrossesDto();
        var days = req.StartDate.DaysBetween(req.EndDate);
        var dict = days.ToDictionary(
            k => (k.Year, k.Month, k.Day), 
            m => new DailyGrossDto(m.Year, m.Month, m.Day));
        
        var filteredLoads = _tenantRepository.ApplySpecification(spec).ToArray();

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

        dailyGrosses.Data = dict.Values;
        return ResponseResult<DailyGrossesDto>.CreateSuccess(dailyGrosses);
    }
}

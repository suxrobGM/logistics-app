using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetMonthlyGrossesHandler : RequestHandler<GetMonthlyGrossesQuery, ResponseResult<MonthlyGrossesDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetMonthlyGrossesHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override async Task<ResponseResult<MonthlyGrossesDto>> HandleValidated(
        GetMonthlyGrossesQuery req, CancellationToken cancellationToken)
    {
        var truckId = req.TruckId;
        
        if (!string.IsNullOrEmpty(req.UserId))
        {
            var driver = await _tenantRepository.GetAsync<Employee>(req.UserId);

            if (driver is null)
            {
                return ResponseResult<MonthlyGrossesDto>.CreateError($"Could not find user with ID '{req.UserId}'");
            }
            
            truckId = driver.TruckId;
        }
        
        var spec = new FilterLoadsByInterval(truckId, req.StartDate, req.EndDate);
        var monthlyGrosses = new MonthlyGrossesDto();
        var months = req.StartDate.MonthsBetween(req.EndDate);
        var filteredLoads = _tenantRepository.ApplySpecification(spec).ToArray();

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

        monthlyGrosses.Data = dict.Values;
        return ResponseResult<MonthlyGrossesDto>.CreateSuccess(monthlyGrosses);
    }
}

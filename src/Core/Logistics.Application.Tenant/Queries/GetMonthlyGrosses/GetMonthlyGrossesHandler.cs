using Logistics.Models;

namespace Logistics.Application.Tenant.Queries;

internal sealed class GetMonthlyGrossesHandler : RequestHandler<GetMonthlyGrossesQuery, ResponseResult<MonthlyGrossesDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetMonthlyGrossesHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    protected override Task<ResponseResult<MonthlyGrossesDto>> HandleValidated(
        GetMonthlyGrossesQuery req, CancellationToken cancellationToken)
    {
        var spec = new FilterLoadsByInterval(req.TruckId, req.StartDate, req.EndDate);
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
            dict[key].Income += load.DeliveryCost;
        }

        monthlyGrosses.Months = dict.Values;
        return Task.FromResult(ResponseResult<MonthlyGrossesDto>.CreateSuccess(monthlyGrosses));
    }

    protected override bool Validate(GetMonthlyGrossesQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (request.StartDate > request.EndDate)
        {
            errorDescription = "The `StartDate` must be less than the `EndDate`";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
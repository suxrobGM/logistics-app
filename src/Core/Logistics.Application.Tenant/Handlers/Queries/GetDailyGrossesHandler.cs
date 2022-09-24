namespace Logistics.Application.Handlers.Queries;

internal sealed class GetDailyGrossesHandler : RequestHandlerBase<GetDailyGrossesQuery, DataResult<DailyGrossesDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetDailyGrossesHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    protected override Task<DataResult<DailyGrossesDto>> HandleValidated(
        GetDailyGrossesQuery req, CancellationToken cancellationToken)
    {
        var spec = new FilterLoadsByInterval(req.StartDate, req.EndDate);
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
            
            dict[key].Income += load.DeliveryCost;
            dict[key].Distance += load.Distance;
        }

        dailyGrosses.Days = dict.Values;
        return Task.FromResult(DataResult<DailyGrossesDto>.CreateSuccess(dailyGrosses));
    }

    protected override bool Validate(GetDailyGrossesQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;
        
        if (request.StartDate > request.EndDate)
        {
            errorDescription = "The `StartDate` must be less than the `EndDate`";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
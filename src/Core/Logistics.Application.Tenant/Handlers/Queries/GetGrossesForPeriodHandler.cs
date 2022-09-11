namespace Logistics.Application.Handlers.Queries;

internal sealed class GetGrossesForPeriodHandler : RequestHandlerBase<GetGrossesForPeriodQuery, DataResult<GrossesPerDayDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetGrossesForPeriodHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    protected override Task<DataResult<GrossesPerDayDto>> HandleValidated(
        GetGrossesForPeriodQuery req, CancellationToken cancellationToken)
    {
        var startPeriod = req.StartPeriod.ToDateOnly();
        var endPeriod = req.EndPeriod.ToDateOnly();
        var spec = new FilterLoadsForPeriod(startPeriod, endPeriod);
        
        var dailyGrossesDict = new Dictionary<string, DailyGross>();
        var interval = req.EndPeriod.Subtract(req.StartPeriod).Days;
        
        var filteredLoads = _tenantRepository.ApplySpecification(spec).ToArray();

        for (var i = 1; i <= interval; i++)
        {
            var date = req.StartPeriod.AddDays(i);
            dailyGrossesDict.Add(date.ToShortDateString(), new DailyGross(date));
        }

        foreach (var load in filteredLoads)
        {
            var date = load.DeliveryDate?.ToShortDateString() ?? "";

            if (!dailyGrossesDict.ContainsKey(date)) 
                continue;
            
            dailyGrossesDict[date].Gross += load.DeliveryCost;
            dailyGrossesDict[date].Distance += load.Distance;
        }

        var grossesPerDay = new GrossesPerDayDto
        {
            Days = dailyGrossesDict.Values
        };

        return Task.FromResult(DataResult<GrossesPerDayDto>.CreateSuccess(grossesPerDay));
    }

    protected override bool Validate(GetGrossesForPeriodQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;
        
        if (request.StartPeriod >= request.EndPeriod)
        {
            errorDescription = "The `StartPeriod` must be less than or equal to `EndPeriod`";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
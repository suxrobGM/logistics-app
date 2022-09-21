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
        var startDate = req.StartDate.ToDateOnly();
        var endDate = req.EndDate.ToDateOnly();
        var spec = new FilterLoadsByInterval(startDate, endDate);
        
        var dailyGrossesDict = new Dictionary<string, DailyGrossDto>();
        var days = req.EndDate.Subtract(req.StartDate).Days;
        
        var filteredLoads = _tenantRepository.ApplySpecification(spec).ToArray();

        for (var i = 1; i <= days; i++)
        {
            var date = req.StartDate.AddDays(i);
            dailyGrossesDict.Add(date.ToShortDateString(), new DailyGrossDto(date));
        }

        foreach (var load in filteredLoads)
        {
            var date = load.DeliveryDate?.ToShortDateString() ?? "";

            if (!dailyGrossesDict.ContainsKey(date)) 
                continue;
            
            dailyGrossesDict[date].Gross += load.DeliveryCost;
            dailyGrossesDict[date].Distance += load.Distance;
        }

        var grossesForInterval = new DailyGrossesDto
        {
            Days = dailyGrossesDict.Values
        };

        return Task.FromResult(DataResult<DailyGrossesDto>.CreateSuccess(grossesForInterval));
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
namespace Logistics.Application.Handlers.Queries;

internal sealed class GetTruckGrossesHandler : RequestHandlerBase<GetTruckGrossesQuery, DataResult<TruckGrossesDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetTruckGrossesHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }
    
    protected override async Task<DataResult<TruckGrossesDto>> HandleValidated(
        GetTruckGrossesQuery req, CancellationToken cancellationToken)
    {
        var truck = await _tenantRepository.GetAsync<Truck>(req.TruckId);

        if (truck == null)
            return DataResult<TruckGrossesDto>.CreateError("Could not find the specified truck");

        var startDate = req.StartDate.ToDateOnly();
        var endDate = req.EndDate.ToDateOnly();
        var spec = new FilterLoadsByIntervalAndTruck(truck.Id, startDate, endDate);
        
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

        var sum = truck.Loads
            .Where(i => i.DeliveryDate.HasValue)
            .GroupBy(_ => 1)
            .Select(i => new
            {
                TotalDistance = i.Sum(m => m.Distance),
                TotalGross = i.Sum(m => m.DeliveryCost)
            })
            .FirstOrDefault();

        var truckGrosses = new TruckGrossesDto
        {
            TruckId = truck.Id,
            Grosses = grossesForInterval,
            TotalDistanceAllTime = sum?.TotalDistance ?? 0,
            TotalGrossAllTime = sum?.TotalGross ?? 0
        };

        return DataResult<TruckGrossesDto>.CreateSuccess(truckGrosses);
    }

    protected override bool Validate(GetTruckGrossesQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;
        
        if (request.StartDate > request.EndDate)
        {
            errorDescription = "The `StartDate` must be less than the `EndDate`";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
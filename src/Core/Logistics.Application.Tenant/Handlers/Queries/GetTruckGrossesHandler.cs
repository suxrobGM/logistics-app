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
        
        var spec = new FilterLoadsByIntervalAndTruck(truck.Id, req.StartDate, req.EndDate);
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
        }

        dailyGrosses.Days = dict.Values;
        var totalDistanceAllTime = 0d;
        var totalGrossAllTime = 0d;

        foreach (var load in truck.Loads.Where(i => i.DeliveryDate.HasValue))
        {
            totalDistanceAllTime += load.Distance;
            totalGrossAllTime += load.DeliveryCost;
        }

        var truckGrosses = new TruckGrossesDto
        {
            TruckId = truck.Id,
            Grosses = dailyGrosses,
            TotalDistanceAllTime = totalDistanceAllTime,
            TotalGrossAllTime = totalGrossAllTime
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
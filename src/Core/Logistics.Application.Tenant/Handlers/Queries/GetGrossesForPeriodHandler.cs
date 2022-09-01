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
        var startPeriod = req.StartPeriod!.Value.ToDateOnly();
        var endPeriod = req.EndPeriod.ToDateOnly();
        var spec = new FilterLoadsForPeriod(startPeriod, endPeriod);
        var loads = _tenantRepository.ApplySpecification(spec).ToArray();

        var dailyGrosses = loads
            .Where(i => i.DeliveryDate.HasValue)
            .Select(i => new DailyGross(i.DeliveryDate!.Value.ToDateTime(), i.DeliveryCost))
            .ToList();
        
        var grossesPerDay = new GrossesPerDayDto
        {
            Days = dailyGrosses
        };

        return Task.FromResult(DataResult<GrossesPerDayDto>.CreateSuccess(grossesPerDay));
    }

    protected override bool Validate(GetGrossesForPeriodQuery request, out string errorDescription)
    {
        errorDescription = string.Empty;

        if (request.StartPeriod == null)
        {
            errorDescription = "Specify the start period of the gross";
        }
        else if (request.StartPeriod > request.EndPeriod)
        {
            errorDescription = "The starting period must be less than end period of the gross";
        }

        return string.IsNullOrEmpty(errorDescription);
    }
}
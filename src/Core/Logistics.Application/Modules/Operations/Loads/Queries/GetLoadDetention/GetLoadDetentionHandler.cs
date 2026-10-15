using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

internal sealed class GetLoadDetentionHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetLoadDetentionQuery, Result<List<StopDetentionDto>>>
{
    public async Task<Result<List<StopDetentionDto>>> Handle(GetLoadDetentionQuery req, CancellationToken ct)
    {
        var load = await tenantUow.Repository<Load>().GetByIdAsync(req.LoadId, ct);
        if (load is null)
        {
            return Result<List<StopDetentionDto>>.Fail($"Could not find a load with ID '{req.LoadId}'");
        }

        var terms = DetentionTerms.For(tenantUow.GetCurrentTenant().Settings, load.Customer);
        var now = DateTime.UtcNow;

        var stops = load.TripStops
            .Where(s => s.ArrivedAt is not null)
            .OrderBy(s => s.Order)
            .Select(s =>
            {
                var detention = DetentionCalculator.Calculate(s, now, terms)!;
                return new StopDetentionDto
                {
                    TripStopId = s.Id,
                    StopType = s.Type,
                    ArrivedAt = s.ArrivedAt!.Value,
                    DepartedAt = s.DepartedAt,
                    DwellMinutes = detention.DwellMinutes,
                    FreeMinutes = detention.FreeMinutes,
                    OverMinutes = detention.OverMinutes,
                    BillableHours = detention.BillableHours,
                    RatePerHour = detention.RatePerHour,
                    Charge = detention.Charge,
                    IsOngoing = detention.IsOngoing,
                    IsBilled = load.FindDetention(s.Id) is not null
                };
            })
            .ToList();

        return Result<List<StopDetentionDto>>.Ok(stops);
    }
}

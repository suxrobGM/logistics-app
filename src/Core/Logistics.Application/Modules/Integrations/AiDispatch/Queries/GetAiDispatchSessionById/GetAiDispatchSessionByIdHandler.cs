using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Queries;

internal sealed class GetAiDispatchSessionByIdHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetAiDispatchSessionByIdQuery, Result<AiDispatchSessionDto>>
{
    public async Task<Result<AiDispatchSessionDto>> Handle(
        GetAiDispatchSessionByIdQuery request, CancellationToken ct)
    {
        var session = await tenantUow.Repository<AiDispatchSession>()
            .GetByIdAsync(request.SessionId, ct);

        if (session is null)
            return Result<AiDispatchSessionDto>.Fail("Session not found");

        var dto = session.ToDtoWithDecisions();

        // Enrich decisions with load names and truck numbers
        var loadIds = dto.Decisions.Where(d => d.LoadId is not null).Select(d => d.LoadId!.Value).Distinct().ToList();
        var truckIds = dto.Decisions.Where(d => d.TruckId is not null).Select(d => d.TruckId!.Value).Distinct().ToList();

        var loadNames = loadIds.Count > 0
            ? (await tenantUow.Repository<Load>().GetListAsync(l => loadIds.Contains(l.Id), ct))
                .ToDictionary(l => l.Id, l => l.Name)
            : [];

        var truckNumbers = truckIds.Count > 0
            ? (await tenantUow.Repository<Truck>().GetListAsync(t => truckIds.Contains(t.Id), ct))
                .ToDictionary(t => t.Id, t => t.Number)
            : [];

        foreach (var d in dto.Decisions)
        {
            if (d.LoadId is not null && loadNames.TryGetValue(d.LoadId.Value, out var ln))
                d.LoadName = ln;
            if (d.TruckId is not null && truckNumbers.TryGetValue(d.TruckId.Value, out var tn))
                d.TruckNumber = tn;
        }

        return Result<AiDispatchSessionDto>.Ok(dto);
    }
}

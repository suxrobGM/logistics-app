using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

internal sealed class GetPendingDecisionsHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetPendingDecisionsQuery, Result<List<DispatchDecisionDto>>>
{
    public async Task<Result<List<DispatchDecisionDto>>> Handle(
        GetPendingDecisionsQuery request, CancellationToken ct)
    {
        var decisions = await tenantUow.Repository<DispatchDecision>()
            .GetListAsync(d => d.Status == DispatchDecisionStatus.Suggested, ct);

        var dtos = decisions
            .OrderByDescending(d => d.CreatedAt)
            .Select(d => new DispatchDecisionDto
            {
                Id = d.Id,
                SessionId = d.SessionId,
                Type = d.Type,
                Status = d.Status,
                Reasoning = d.Reasoning,
                ToolName = d.ToolName,
                ToolInput = d.ToolInput,
                LoadId = d.LoadId,
                TruckId = d.TruckId,
                TripId = d.TripId,
                CreatedAt = d.CreatedAt
            })
            .ToList();

        return Result<List<DispatchDecisionDto>>.Ok(dtos);
    }
}

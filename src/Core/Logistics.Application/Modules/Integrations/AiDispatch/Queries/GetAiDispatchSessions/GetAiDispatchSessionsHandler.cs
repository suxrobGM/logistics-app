using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AiDispatch.Queries;

internal sealed class GetAiDispatchSessionsHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetAiDispatchSessionsQuery, PagedResult<AiDispatchSessionDto>>
{
    public async Task<PagedResult<AiDispatchSessionDto>> Handle(
        GetAiDispatchSessionsQuery request, CancellationToken ct)
    {
        var query = tenantUow.Repository<AiDispatchSession>().Query();

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        var totalItems = await query.CountAsync(ct);

        var sessions = await query
            .OrderByDescending(s => s.StartedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var dtos = sessions.Select(s => s.ToDto()).ToList();

        return PagedResult<AiDispatchSessionDto>.Ok(dtos, totalItems, request.PageSize);
    }
}

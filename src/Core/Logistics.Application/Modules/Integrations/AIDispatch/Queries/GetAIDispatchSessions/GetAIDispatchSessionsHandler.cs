using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Integrations.AIDispatch.Queries;

internal sealed class GetAIDispatchSessionsHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetAIDispatchSessionsQuery, PagedResult<AIDispatchSessionDto>>
{
    public async Task<PagedResult<AIDispatchSessionDto>> Handle(
        GetAIDispatchSessionsQuery request, CancellationToken ct)
    {
        var query = tenantUow.Repository<AIDispatchSession>().Query();

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        var totalItems = await query.CountAsync(ct);

        var sessions = await query
            .OrderByDescending(s => s.StartedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var dtos = sessions.Select(s => s.ToDto()).ToList();

        return PagedResult<AIDispatchSessionDto>.Ok(dtos, totalItems, request.PageSize);
    }
}

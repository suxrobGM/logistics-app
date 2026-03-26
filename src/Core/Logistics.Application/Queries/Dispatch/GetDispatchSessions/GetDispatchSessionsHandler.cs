using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetDispatchSessionsHandler(
    ITenantUnitOfWork tenantUow) : IAppRequestHandler<GetDispatchSessionsQuery, PagedResult<DispatchSessionDto>>
{
    public async Task<PagedResult<DispatchSessionDto>> Handle(
        GetDispatchSessionsQuery request, CancellationToken ct)
    {
        var query = tenantUow.Repository<DispatchSession>().Query();

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        var totalItems = await query.CountAsync(ct);

        var sessions = await query
            .OrderByDescending(s => s.StartedAt)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var dtos = sessions.Select(s => s.ToDto()).ToList();

        return PagedResult<DispatchSessionDto>.Ok(dtos, totalItems, request.PageSize);
    }
}

using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Compliance.Eld.Queries;

internal sealed class GetDriverHosLogsHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetDriverHosLogsQuery, PagedResult<HosLogDto>>
{
    public async Task<PagedResult<HosLogDto>> Handle(
        GetDriverHosLogsQuery req,
        CancellationToken ct)
    {
        var query = tenantUow.Repository<HosLog>()
            .Query()
            .Where(l => l.EmployeeId == req.EmployeeId);

        query = query.Where(l => l.LogDate >= DateOnly.FromDateTime(req.FromDate) && l.LogDate <= DateOnly.FromDateTime(req.ToDate));

        var totalItems = await query.CountAsync(ct);

        var logs = await query
            .OrderByDescending(l => l.StartedAt)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(l => l.ToDto(l.Employee.FirstName + " " + l.Employee.LastName))
            .ToArrayAsync(ct);

        return PagedResult<HosLogDto>.Ok(logs, totalItems, req.PageSize);
    }
}

using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetDriverHosLogsHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetDriverHosLogsQuery, PagedResult<HosLogDto>>
{
    public async Task<PagedResult<HosLogDto>> Handle(
        GetDriverHosLogsQuery req,
        CancellationToken ct)
    {
        var query = tenantUow.Repository<HosLog>()
            .Query()
            .Where(l => l.EmployeeId == req.EmployeeId);

        query = query.Where(l => l.LogDate >= req.StartDate && l.LogDate <= req.EndDate);

        var totalItems = await query.CountAsync(ct);

        var logs = await query
            .OrderByDescending(l => l.StartTime)
            .Skip((req.Page - 1) * req.PageSize)
            .Take(req.PageSize)
            .Select(l => l.ToDto(l.Employee.FirstName + " " + l.Employee.LastName))
            .ToArrayAsync(ct);

        return PagedResult<HosLogDto>.Succeed(logs, totalItems, req.PageSize);
    }
}

using Logistics.Application.Abstractions;
using Logistics.Application.Common.Extensions;
using Logistics.Application.Common.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Operations.TimeEntries.Queries;

internal sealed class GetTimeEntriesHandler(ITenantUnitOfWork tenantUow, IDisplayNameLookup names)
    : IRequestHandler<GetTimeEntriesQuery, PagedResult<TimeEntryDto>>
{
    public async Task<PagedResult<TimeEntryDto>> Handle(GetTimeEntriesQuery req, CancellationToken ct)
    {
        var repository = tenantUow.Repository<TimeEntry>();
        var query = repository.Query();

        if (req.EmployeeId.HasValue)
        {
            query = query.Where(t => t.EmployeeId == req.EmployeeId.Value);
        }

        if (req.FromDate.HasValue)
        {
            query = query.Where(t => t.StartedAt >= req.FromDate.Value);
        }
        if (req.ToDate.HasValue)
        {
            // ToDate is a date and inclusive, so keep entries that start before the following midnight.
            query = query.Where(t => t.StartedAt < req.ToDate.Value.AddDays(1));
        }

        if (req.Type.HasValue)
        {
            query = query.Where(t => t.Type == req.Type.Value);
        }

        return await query.ToPagedResultAsync(req, async (page, token) =>
        {
            var employeeNames = await names.GetEmployeeNamesAsync(page.Select(t => (Guid?)t.EmployeeId), token);
            return page.Select(t => t.ToDto(employeeNames.Find(t.EmployeeId)));
        }, ct, defaultOrderBy: "-StartedAt");
    }
}

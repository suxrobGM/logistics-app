using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;
using Logistics.Application.Modules.Operations.Common.Services;
using Logistics.Application.Utilities;

namespace Logistics.Application.Modules.Operations.TimeEntries.Queries;

internal sealed class GetTimeEntriesHandler(ITenantUnitOfWork tenantUow, IDisplayNameLookup names)
    : IRequestHandler<GetTimeEntriesQuery, PagedResult<TimeEntryDto>>
{
    public async Task<PagedResult<TimeEntryDto>> Handle(GetTimeEntriesQuery req, CancellationToken ct)
    {
        var repository = tenantUow.Repository<TimeEntry>();
        var query = repository.Query();

        // Filter by employee
        if (req.EmployeeId.HasValue)
        {
            query = query.Where(t => t.EmployeeId == req.EmployeeId.Value);
        }

        // Filter by date range
        if (req.StartDate.HasValue)
        {
            query = query.Where(t => t.Date >= req.StartDate.Value);
        }
        if (req.EndDate.HasValue)
        {
            query = query.Where(t => t.Date <= req.EndDate.Value);
        }

        // Filter by type
        if (!string.IsNullOrWhiteSpace(req.Type) && Enum.TryParse<TimeEntryType>(req.Type, true, out var type))
        {
            query = query.Where(t => t.Type == type);
        }

        // Filter by linked to payroll
        if (req.IncludeLinkedToPayroll.HasValue && !req.IncludeLinkedToPayroll.Value)
        {
            query = query.Where(t => t.PayrollInvoiceId == null);
        }

        return await query.ToPagedResultAsync(req, async (page, token) =>
        {
            var employeeNames = await names.GetEmployeeNamesAsync(page.Select(t => (Guid?)t.EmployeeId), token);
            return page.Select(t => t.ToDto(employeeNames.Find(t.EmployeeId)));
        }, ct, defaultOrderBy: "-Date");
    }
}

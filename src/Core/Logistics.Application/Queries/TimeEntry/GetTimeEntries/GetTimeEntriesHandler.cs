using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Queries;

internal sealed class GetTimeEntriesHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<GetTimeEntriesQuery, PagedResult<TimeEntryDto>>
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

        // Get total count for pagination
        var totalItems = await query.CountAsync(ct);

        // Apply sorting
        if (!string.IsNullOrWhiteSpace(req.OrderBy))
        {
            var isDescending = req.OrderBy.StartsWith("-");
            var sortField = isDescending ? req.OrderBy[1..] : req.OrderBy;

            query = sortField.ToLowerInvariant() switch
            {
                "date" => isDescending ? query.OrderByDescending(t => t.Date) : query.OrderBy(t => t.Date),
                "totalhours" => isDescending ? query.OrderByDescending(t => t.TotalHours) : query.OrderBy(t => t.TotalHours),
                "createdat" => isDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt),
                _ => query.OrderByDescending(t => t.Date)
            };
        }
        else
        {
            query = query.OrderByDescending(t => t.Date);
        }

        // Apply pagination
        var skip = (req.Page - 1) * req.PageSize;
        query = query.Skip(skip).Take(req.PageSize);

        var timeEntries = query.ToList();
        var dtos = timeEntries.Select(t => t.ToDto()).ToArray();

        return PagedResult<TimeEntryDto>.Succeed(dtos, totalItems, req.PageSize);
    }
}

using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Application.Modules.Financial.Payroll.Queries;

internal sealed class GetCashAdvancesHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<GetCashAdvancesQuery, Result<List<CashAdvanceDto>>>
{
    private const int Limit = 200;

    public async Task<Result<List<CashAdvanceDto>>> Handle(GetCashAdvancesQuery req, CancellationToken ct)
    {
        var query = tenantUow.Repository<CashAdvance>().Query();

        if (req.EmployeeId is { } employeeId)
        {
            query = query.Where(a => a.EmployeeId == employeeId);
        }

        var rows = await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(Limit)
            .Select(a => new { Advance = a, Name = a.Employee.FirstName + " " + a.Employee.LastName })
            .ToListAsync(ct);

        return Result<List<CashAdvanceDto>>.Ok(rows.Select(r => r.Advance.ToDto(r.Name)).ToList());
    }
}

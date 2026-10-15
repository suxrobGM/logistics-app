using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class IssueCashAdvanceHandler(ITenantUnitOfWork tenantUow, ICurrentUserService currentUser)
    : IRequestHandler<IssueCashAdvanceCommand, Result<CashAdvanceDto>>
{
    public async Task<Result<CashAdvanceDto>> Handle(IssueCashAdvanceCommand req, CancellationToken ct)
    {
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId, ct);
        if (employee is null)
        {
            return Result<CashAdvanceDto>.Fail($"Could not find an employee with ID '{req.EmployeeId}'");
        }

        var currency = tenantUow.GetCurrentTenant().Settings.Currency.ToString();
        var advance = new CashAdvance
        {
            EmployeeId = employee.Id,
            Amount = new() { Amount = req.Amount, Currency = currency },
            Reason = req.Reason
        };
        advance.Approve(currentUser.GetUserId() ?? Guid.Empty);

        await tenantUow.Repository<CashAdvance>().AddAsync(advance, ct);
        await tenantUow.SaveChangesAsync(ct);
        return Result<CashAdvanceDto>.Ok(advance.ToDto(employee.GetFullName()));
    }
}

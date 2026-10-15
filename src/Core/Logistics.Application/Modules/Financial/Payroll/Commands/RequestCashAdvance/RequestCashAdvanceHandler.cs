using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.Notifications;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Mappings;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class RequestCashAdvanceHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUser,
    INotificationService notificationService)
    : IRequestHandler<RequestCashAdvanceCommand, Result<CashAdvanceDto>>
{
    public async Task<Result<CashAdvanceDto>> Handle(RequestCashAdvanceCommand req, CancellationToken ct)
    {
        var employeeId = currentUser.GetUserId();
        var employee = employeeId is null ? null : await tenantUow.Repository<Employee>().GetByIdAsync(employeeId.Value, ct);
        if (employee is null)
        {
            return Result<CashAdvanceDto>.Fail("Only an employee of this company can request an advance.");
        }

        var currency = (tenantUow.GetCurrentTenant().Settings?.Currency ?? CurrencyCode.USD).ToString();
        var advance = new CashAdvance
        {
            EmployeeId = employee.Id,
            Amount = new() { Amount = req.Amount, Currency = currency },
            Reason = req.Reason
        };

        await tenantUow.Repository<CashAdvance>().AddAsync(advance, ct);
        await tenantUow.SaveChangesAsync(ct);

        await notificationService.SendNotificationAsync(
            "Cash advance requested",
            $"{employee.GetFullName()} asked for an advance of {req.Amount:0.00} {currency}.");

        return Result<CashAdvanceDto>.Ok(advance.ToDto(employee.GetFullName()));
    }
}

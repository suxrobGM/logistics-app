using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Mediator;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payroll.Commands;

internal sealed class UpdateDriverPayProfileHandler(ITenantUnitOfWork tenantUow)
    : IRequestHandler<UpdateDriverPayProfileCommand, Result>
{
    public async Task<Result> Handle(UpdateDriverPayProfileCommand req, CancellationToken ct)
    {
        var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId, ct);
        if (employee is null)
        {
            return Result.Fail($"Could not find an employee with ID '{req.EmployeeId}'");
        }

        employee.PayExcludesFuelSurcharge = req.PayExcludesFuelSurcharge;
        employee.PayExcludesAccessorials = req.PayExcludesAccessorials;
        employee.DeductFuelCardCharges = req.DeductFuelCardCharges;

        var repository = tenantUow.Repository<RecurringDeduction>();
        var existing = await repository.GetListAsync(d => d.EmployeeId == employee.Id, ct);
        var keep = req.RecurringDeductions.Where(d => d.Id is not null).Select(d => d.Id!.Value).ToHashSet();

        foreach (var stale in existing.Where(d => !keep.Contains(d.Id)))
        {
            repository.Delete(stale);
        }

        foreach (var input in req.RecurringDeductions)
        {
            var deduction = existing.FirstOrDefault(d => d.Id == input.Id);
            if (deduction is null)
            {
                deduction = new RecurringDeduction
                {
                    EmployeeId = employee.Id,
                    Type = input.Type,
                    Description = input.Description
                };
                await repository.AddAsync(deduction, ct);
            }

            deduction.Type = input.Type;
            deduction.Description = input.Description.Trim();
            deduction.Amount = input.Amount;
            deduction.EscrowTarget = input.EscrowTarget;
        }

        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}

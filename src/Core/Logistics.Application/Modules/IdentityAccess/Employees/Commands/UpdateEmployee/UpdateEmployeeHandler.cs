using Logistics.Application.Abstractions;
using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Identity.Roles;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

internal sealed class UpdateEmployeeHandler(
    ITenantUnitOfWork tenantUow,
    ICurrentUserService currentUserService)
    : IAppRequestHandler<UpdateEmployeeCommand, Result>
{
    public async Task<Result> Handle(
        UpdateEmployeeCommand req, CancellationToken ct)
    {
        var employeeEntity = await tenantUow.Repository<Employee>().GetByIdAsync(req.UserId, ct);
        var tenantRole = await tenantUow.Repository<TenantRole>().GetAsync(i => i.Name == req.Role, ct);

        if (employeeEntity is null)
        {
            return Result.Fail("Could not find the specified user");
        }

        // Role changes are privilege-sensitive: guard against self-escalation and against assigning
        // a role that outranks the caller's own. Without this, any Employee.Manage holder (e.g. a
        // Manager) could promote themselves - or anyone - to Owner and gain the whole tenant.
        if (tenantRole is not null && tenantRole.Name != employeeEntity.Role?.Name)
        {
            var guard = await CheckRoleChangeAllowedAsync(req.UserId, employeeEntity, tenantRole, ct);
            if (!guard.IsSuccess)
            {
                return guard;
            }

            employeeEntity.Role = tenantRole;
        }

        if (req.SalaryType.HasValue && employeeEntity.SalaryType != req.SalaryType)
        {
            employeeEntity.SalaryType = req.SalaryType.Value;
        }

        if (req.Salary.HasValue && employeeEntity.Salary != req.Salary)
        {
            var salaryAmount = req.SalaryType == SalaryType.None ? 0 : req.Salary.Value;
            employeeEntity.Salary = new() { Amount = salaryAmount, Currency = employeeEntity.Salary.Currency };
        }

        if (req.Status.HasValue && employeeEntity.Status != req.Status)
        {
            employeeEntity.Status = req.Status.Value;
        }

        if (req.Address is not null)
        {
            employeeEntity.Address = req.Address;
        }

        tenantUow.Repository<Employee>().Update(employeeEntity);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private async Task<Result> CheckRoleChangeAllowedAsync(
        Guid targetUserId, Employee target, TenantRole newRole, CancellationToken ct)
    {
        var callerId = currentUserService.GetUserId();
        if (callerId is null)
        {
            return Result.Fail("User not authenticated.");
        }

        // No one may change their own role - that is the self-escalation path.
        if (callerId.Value == targetUserId)
        {
            return Result.Fail("You cannot change your own role.");
        }

        var caller = await tenantUow.Repository<Employee>().GetByIdAsync(callerId.Value, ct);
        var callerRank = RoleRank(caller?.Role?.Name);
        var newRoleRank = RoleRank(newRole.Name);
        var targetCurrentRank = RoleRank(target.Role?.Name);

        // A caller can neither grant a role above their own level nor act on someone who already
        // outranks them (which would let a Manager demote an Owner).
        if (newRoleRank > callerRank || targetCurrentRank > callerRank)
        {
            return Result.Fail("You cannot assign a role higher than your own.");
        }

        return Result.Ok();
    }

    /// <summary>
    /// Privilege ordering of the built-in tenant roles. Higher wins. Unknown/custom roles rank
    /// lowest so they can never be used to out-rank a built-in role.
    /// </summary>
    private static int RoleRank(string? roleName) => roleName switch
    {
        TenantRoles.Owner => 4,
        TenantRoles.Manager => 3,
        TenantRoles.Dispatcher => 2,
        TenantRoles.Driver => 1,
        TenantRoles.Customer => 0,
        _ => -1
    };
}

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

        // Without a rank check any Employee.Manage holder could promote themselves to Owner.
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

        if (callerId.Value == targetUserId)
        {
            return Result.Fail("You cannot change your own role.");
        }

        var caller = await tenantUow.Repository<Employee>().GetByIdAsync(callerId.Value, ct);

        // A custom role's name says nothing about the permissions behind it, so a guessed rank
        // could hand out more authority than the caller has. Refuse anything unranked.
        if (RoleRank(newRole.Name) is not { } newRoleRank ||
            RoleRank(caller?.Role?.Name) is not { } callerRank ||
            RoleRank(target.Role?.Name) is not { } targetRank)
        {
            return Result.Fail("This role cannot be assigned here.");
        }

        // Also blocks acting on someone who already outranks you, e.g. a Manager demoting an Owner.
        if (newRoleRank > callerRank || targetRank > callerRank)
        {
            return Result.Fail("You cannot assign a role higher than your own.");
        }

        return Result.Ok();
    }

    /// <summary>
    /// Privilege ordering of the built-in tenant roles, highest first. Null for anything else,
    /// including a custom role, whose authority cannot be inferred from its name.
    /// </summary>
    private static int? RoleRank(string? roleName) => roleName switch
    {
        TenantRoles.Owner => 4,
        TenantRoles.Manager => 3,
        TenantRoles.Dispatcher => 2,
        TenantRoles.Driver => 1,
        TenantRoles.Customer => 0,
        _ => null
    };
}

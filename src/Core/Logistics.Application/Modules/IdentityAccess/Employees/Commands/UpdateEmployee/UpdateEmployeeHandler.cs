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
        if (currentUserService.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin))
        {
            return Result.Ok();
        }

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
        if (caller?.Role is null)
        {
            return Result.Fail("This role cannot be assigned here.");
        }

        var callerClaims = caller.Role.Claims
            .Select(c => (c.ClaimType, c.ClaimValue))
            .ToHashSet();

        if (HasClaimsOutside(newRole, callerClaims) ||
            target.Role is not null && HasClaimsOutside(target.Role, callerClaims))
        {
            return Result.Fail("You cannot assign a role with permissions beyond your own.");
        }

        return Result.Ok();
    }

    private static bool HasClaimsOutside(
        TenantRole role,
        HashSet<(string ClaimType, string ClaimValue)> allowedClaims) =>
        role.Claims.Any(c => !allowedClaims.Contains((c.ClaimType, c.ClaimValue)));
}

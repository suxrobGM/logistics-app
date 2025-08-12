using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateUserHandler : RequestHandler<UpdateUserCommand, Result>
{
    private readonly IMasterUnitOfWork _masterUow;
    private readonly ITenantUnitOfWork _tenantUow;

    public UpdateUserHandler(
        IMasterUnitOfWork masterUow,
        ITenantUnitOfWork tenantUow)
    {
        _masterUow = masterUow;
        _tenantUow = tenantUow;
    }

    public override async Task<Result> Handle(
        UpdateUserCommand req, CancellationToken ct)
    {
        var user = await _masterUow.Repository<User>().GetByIdAsync(req.Id);

        if (user is null)
        {
            return Result.Fail("Could not find the specified user");
        }

        if (!string.IsNullOrEmpty(req.FirstName))
        {
            user.FirstName = req.FirstName;
        }

        if (!string.IsNullOrEmpty(req.LastName))
        {
            user.LastName = req.LastName;
        }

        if (!string.IsNullOrEmpty(req.PhoneNumber))
        {
            user.PhoneNumber = req.PhoneNumber;
        }

        if (req.TenantId.HasValue)
        {
            await UpdateTenantEmployeeDataAsync(req.TenantId.Value, user);
        }

        _masterUow.Repository<User>().Update(user);
        await _masterUow.SaveChangesAsync();
        await _tenantUow.SaveChangesAsync();
        return Result.Succeed();
    }

    private async Task UpdateTenantEmployeeDataAsync(Guid tenantId, User user)
    {
        _tenantUow.SetCurrentTenantById(tenantId.ToString());
        var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(user.Id);

        if (employee is null)
        {
            return;
        }

        employee.FirstName = user.FirstName;
        employee.LastName = user.LastName;
        employee.Email = user.Email;
        employee.PhoneNumber = user.PhoneNumber;
        _tenantUow.Repository<Employee>().Update(employee);
    }
}

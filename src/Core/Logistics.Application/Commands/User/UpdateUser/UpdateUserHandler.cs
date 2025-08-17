using Logistics.Application.Abstractions;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateUserHandler : IAppRequestHandler<UpdateUserCommand, Result>
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

    public async Task<Result> Handle(
        UpdateUserCommand req, CancellationToken ct)
    {
        var user = await _masterUow.Repository<User>().GetByIdAsync(req.Id, ct);

        if (user is null)
        {
            return Result.Fail("Could not find the specified user");
        }

        user.FirstName = PropertyUpdater.UpdateIfChanged(req.FirstName, user.FirstName);
        user.LastName = PropertyUpdater.UpdateIfChanged(req.LastName, user.LastName);
        user.PhoneNumber = PropertyUpdater.UpdateIfChanged(req.PhoneNumber, user.PhoneNumber);

        if (req.TenantId.HasValue)
        {
            await UpdateTenantEmployeeDataAsync(req.TenantId.Value, user, ct);
        }

        _masterUow.Repository<User>().Update(user);
        await _masterUow.SaveChangesAsync(ct);
        await _tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    private async Task UpdateTenantEmployeeDataAsync(Guid tenantId, User user, CancellationToken ct = default)
    {
        await _tenantUow.SetCurrentTenantByIdAsync(tenantId);
        var employee = await _tenantUow.Repository<Employee>().GetByIdAsync(user.Id, ct);

        if (employee is null)
        {
            return;
        }

        employee.FirstName = user.FirstName;
        employee.LastName = user.LastName;
        employee.Email = user.Email;
        employee.PhoneNumber = user.PhoneNumber;
    }
}

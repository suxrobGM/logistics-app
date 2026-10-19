using Logistics.Application.Abstractions.Tenancy;
using Logistics.Application.Utilities;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.IdentityAccess.Users.Services;

public class UserService(
    IMasterUnitOfWork masterUow,
    IUserTenantReader userTenantReader,
    IServiceScopeFactory scopeFactory) : IUserService
{
    public async Task<bool> UpdateUserAsync(UpdateUserParams userParams, CancellationToken ct = default)
    {
        var user = await masterUow.Repository<User>().GetByIdAsync(userParams.Id, ct);
        if (user is null)
        {
            return false;
        }

        var before = (user.FirstName, user.LastName, user.PhoneNumber);
        user.FirstName = PropertyUpdater.UpdateIfChanged(userParams.FirstName, user.FirstName);
        user.LastName = PropertyUpdater.UpdateIfChanged(userParams.LastName, user.LastName);
        user.PhoneNumber = PropertyUpdater.UpdateIfChanged(userParams.PhoneNumber, user.PhoneNumber);

        if ((user.FirstName, user.LastName, user.PhoneNumber) != before)
        {
            await masterUow.SaveChangesAsync(ct);
            await SyncTenantProfilesAsync(user, ct);
        }

        return true;
    }

    public async Task SyncTenantProfilesAsync(User user, CancellationToken ct = default)
    {
        foreach (var tenant in await userTenantReader.ListAsync(user, ct))
        {
            // A scope per tenant: Employee ids are user ids, so one context would hand back the previous
            // tenant's tracked row from FindAsync.
            using var scope = scopeFactory.CreateScope();
            var tenantUow = scope.ServiceProvider.GetRequiredService<ITenantUnitOfWork>();
            tenantUow.SetCurrentTenant(tenant);

            var employee = await tenantUow.Repository<Employee>().GetByIdAsync(user.Id, ct);
            if (employee is not null)
            {
                employee.Email = user.Email!;
                employee.FirstName = user.FirstName;
                employee.LastName = user.LastName;
                employee.PhoneNumber = user.PhoneNumber;
            }

            var customerUsers = await tenantUow.Repository<CustomerUser>().GetListAsync(c => c.UserId == user.Id, ct);
            foreach (var customerUser in customerUsers)
            {
                customerUser.Email = user.Email!;
            }

            await tenantUow.SaveChangesAsync(ct);
        }
    }
}

using Logistics.Application.Abstractions.Tenancy;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.IdentityAccess.Users.Services;

public class UserService(
    IMasterUnitOfWork masterUow,
    IServiceScopeFactory scopeFactory) : IUserService
{
    public async Task UpdateUserAsync(UpdateUserParams userParams)
    {
        var userRepository = masterUow.Repository<User>();
        var user = await userRepository.GetByIdAsync(userParams.Id);

        if (user is null)
        {
            return;
        }

        if (!string.IsNullOrEmpty(userParams.FirstName))
        {
            user.FirstName = userParams.FirstName;
        }

        if (!string.IsNullOrEmpty(userParams.LastName))
        {
            user.LastName = userParams.LastName;
        }

        if (!string.IsNullOrEmpty(userParams.PhoneNumber))
        {
            user.PhoneNumber = userParams.PhoneNumber;
        }

        await masterUow.SaveChangesAsync();
        await SyncTenantProfilesAsync(user);
    }

    public async Task SyncTenantProfilesAsync(User user, CancellationToken ct = default)
    {
        foreach (var tenant in await UserTenants.ListAsync(masterUow, user, ct))
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

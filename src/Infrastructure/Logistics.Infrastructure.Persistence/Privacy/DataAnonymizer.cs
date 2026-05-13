using Logistics.Application.Abstractions.Privacy;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.Persistence.Privacy;

/// <summary>
/// Replaces a user's directly-identifying fields with placeholder values across
/// the master DB and every tenant DB they belong to. Operational and financial
/// records keep their FKs intact so invoices, payroll, and audit logs continue
/// to balance after anonymization. Idempotent.
/// </summary>
internal sealed class DataAnonymizer(
    IMasterUnitOfWork masterUow,
    ITenantUnitOfWork tenantUow,
    UserManager<User> userManager,
    ILogger<DataAnonymizer> logger) : IDataAnonymizer
{
    private const string DeletedFirstName = "Deleted";
    private const string DeletedLastName = "User";
    private const string DeletedDisplayName = "[deleted]";

    public async Task AnonymizeUserAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await masterUow.Repository<User>().GetByIdAsync(userId, ct);
        if (user is null)
        {
            logger.LogWarning("Anonymize requested for missing user '{UserId}'; skipping.", userId);
            return;
        }

        if (user.AnonymizedAt is not null)
        {
            return;
        }

        await AnonymizeTenantRecordsAsync(user, ct);
        await AnonymizeMasterRecordsAsync(user, ct);

        logger.LogInformation("Anonymized user '{UserId}'.", userId);
    }

    private async Task AnonymizeTenantRecordsAsync(User user, CancellationToken ct)
    {
        var tenants = await masterUow.Repository<Tenant>()
            .Query()
            .Where(t => t.Users.Any(u => u.Id == user.Id))
            .ToListAsync(ct);

        var anonymizedEmail = AnonymizedEmail(user.Id);

        foreach (var tenant in tenants)
        {
            try
            {
                tenantUow.SetCurrentTenant(tenant);

                var employee = await tenantUow.Repository<Employee>().GetByIdAsync(user.Id, ct);
                if (employee is not null)
                {
                    employee.FirstName = DeletedFirstName;
                    employee.LastName = DeletedLastName;
                    employee.Email = anonymizedEmail;
                    employee.PhoneNumber = null;
                    employee.DeviceToken = null;
                }

                var customerUser = await tenantUow.Repository<CustomerUser>()
                    .Query()
                    .FirstOrDefaultAsync(c => c.UserId == user.Id, ct);

                if (customerUser is not null)
                {
                    customerUser.Email = anonymizedEmail;
                    customerUser.DisplayName = DeletedDisplayName;
                    customerUser.IsActive = false;
                }

                await tenantUow.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Failed to anonymize user '{UserId}' in tenant '{TenantName}'. Manual cleanup required.",
                    user.Id, tenant.Name);
                throw;
            }
        }
    }

    private async Task AnonymizeMasterRecordsAsync(User user, CancellationToken ct)
    {
        var anonymizedEmail = AnonymizedEmail(user.Id);

        // UserManager normalizes Email/UserName/PhoneNumber and persists;
        // touching it through the repository directly leaves the indexes stale.
        user.FirstName = DeletedFirstName;
        user.LastName = DeletedLastName;
        user.PhoneNumber = null;
        user.AnonymizedAt = DateTime.UtcNow;
        // Lock out future logins.
        user.LockoutEnabled = true;
        user.LockoutEnd = DateTimeOffset.MaxValue;

        await userManager.SetEmailAsync(user, anonymizedEmail);
        await userManager.SetUserNameAsync(user, anonymizedEmail);
        await userManager.UpdateAsync(user);

        var accesses = await masterUow.Repository<UserTenantAccess>()
            .Query()
            .Where(a => a.UserId == user.Id)
            .ToListAsync(ct);

        foreach (var access in accesses)
        {
            access.CustomerName = DeletedDisplayName;
            access.IsActive = false;
        }

        await masterUow.SaveChangesAsync(ct);
    }

    private static string AnonymizedEmail(Guid userId)
        => $"deleted-{userId.ToString("N")[..8]}@anonymized.local";
}

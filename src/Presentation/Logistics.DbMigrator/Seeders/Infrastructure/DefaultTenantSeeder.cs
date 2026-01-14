using Logistics.Application.Services;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Seeders.Infrastructure;

/// <summary>
/// Seeds the default tenant with upsert logic.
/// </summary>
internal class DefaultTenantSeeder(ILogger<DefaultTenantSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(DefaultTenantSeeder);
    public override SeederType Type => SeederType.Infrastructure;
    public override int Order => 40;

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();
        var repo = context.MasterUnitOfWork.Repository<Tenant>();
        var databaseProvider = context.ServiceProvider.GetRequiredService<ITenantDatabaseService>();

        var defaultTenantConnectionString = context.Configuration.GetConnectionString("DefaultTenantDatabase")
                                            ?? databaseProvider.GenerateConnectionString("default");

        var companyAddress = new Address
        {
            Line1 = "7 Allstate Rd",
            City = "Dorchester",
            State = "Massachusetts",
            ZipCode = "02125",
            Country = "United States"
        };

        var defaultTenant = new Tenant
        {
            Name = "default",
            CompanyName = "Test Company",
            BillingEmail = "test@gmail.com",
            CompanyAddress = companyAddress,
            ConnectionString = defaultTenantConnectionString
        };

        var existingTenant = await repo.GetAsync(i => i.Name == defaultTenant.Name, cancellationToken);

        if (existingTenant is null)
        {
            await repo.AddAsync(defaultTenant, cancellationToken);
            await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
            await databaseProvider.CreateDatabaseAsync(defaultTenant.ConnectionString);
            Logger.LogInformation("Created default tenant with connection string: {ConnectionString}",
                defaultTenant.ConnectionString);

            context.DefaultTenant = defaultTenant;
        }
        else
        {
            // Upsert: update company info if changed
            var updated = false;
            if (existingTenant.CompanyName != defaultTenant.CompanyName)
            {
                existingTenant.CompanyName = defaultTenant.CompanyName;
                updated = true;
            }

            if (existingTenant.BillingEmail != defaultTenant.BillingEmail)
            {
                existingTenant.BillingEmail = defaultTenant.BillingEmail;
                updated = true;
            }

            if (updated)
            {
                await context.MasterUnitOfWork.SaveChangesAsync(cancellationToken);
                Logger.LogInformation("Updated default tenant");
            }
            else
            {
                Logger.LogInformation("Default tenant already up to date");
            }

            context.DefaultTenant = existingTenant;
        }

        LogCompleted();
    }
}

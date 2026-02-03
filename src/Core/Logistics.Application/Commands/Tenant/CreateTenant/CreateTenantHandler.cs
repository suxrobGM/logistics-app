using Logistics.Application.Abstractions;
using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateTenantHandler(
    ITenantDatabaseService tenantDatabase,
    IMasterUnitOfWork masterUow,
    IFeatureService featureService)
    : IAppRequestHandler<CreateTenantCommand, Result>
{
    public async Task<Result> Handle(CreateTenantCommand req, CancellationToken ct)
    {
        var tenantName = req.Name.Trim().ToLower();
        var tenant = new Tenant
        {
            Name = tenantName,
            CompanyName = req.CompanyName,
            DotNumber = req.DotNumber,
            CompanyAddress = req.CompanyAddress,
            BillingEmail = req.BillingEmail!,
            ConnectionString = tenantDatabase.GenerateConnectionString(tenantName)
        };

        var existingTenant = await masterUow.Repository<Tenant>().GetAsync(i => i.Name == tenant.Name, ct);

        if (existingTenant is not null)
        {
            return Result.Fail($"Tenant name '{tenant.Name}' is already taken, please chose another name");
        }

        var created = await tenantDatabase.CreateDatabaseAsync(tenant.ConnectionString);
        if (!created)
        {
            return Result.Fail("Could not create the tenant's database");
        }

        await masterUow.Repository<Tenant>().AddAsync(tenant, ct);
        await masterUow.SaveChangesAsync(ct);

        // Initialize feature configurations for the new tenant based on defaults
        await featureService.InitializeFeaturesForTenantAsync(tenant.Id);

        return Result.Ok();
    }
}

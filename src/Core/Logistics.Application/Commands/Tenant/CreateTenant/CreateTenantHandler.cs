using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Domain.Services;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateTenantHandler : RequestHandler<CreateTenantCommand, Result>
{
    private readonly IMasterUnitOfWork _masterUow;
    private readonly ITenantDatabaseService _tenantDatabase;

    public CreateTenantHandler(
        ITenantDatabaseService tenantDatabase,
        IMasterUnitOfWork masterUow)
    {
        _tenantDatabase = tenantDatabase;
        _masterUow = masterUow;
    }

    protected override async Task<Result> HandleValidated(CreateTenantCommand req, CancellationToken ct)
    {
        var tenantName = req.Name.Trim().ToLower();
        var tenant = new Tenant
        {
            Name = tenantName,
            CompanyName = req.CompanyName,
            DotNumber = req.DotNumber,
            CompanyAddress = req.CompanyAddress ?? Address.NullAddress,
            BillingEmail = req.BillingEmail!,
            ConnectionString = _tenantDatabase.GenerateConnectionString(tenantName)
        };

        var existingTenant = await _masterUow.Repository<Tenant>().GetAsync(i => i.Name == tenant.Name);

        if (existingTenant is not null)
        {
            return Result.Fail($"Tenant name '{tenant.Name}' is already taken, please chose another name");
        }

        var created = await _tenantDatabase.CreateDatabaseAsync(tenant.ConnectionString);
        if (!created)
        {
            return Result.Fail("Could not create the tenant's database");
        }

        await _masterUow.Repository<Tenant>().AddAsync(tenant);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}

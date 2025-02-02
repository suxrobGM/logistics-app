using Logistics.Application;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;
using Logistics.Domain.ValueObjects;
using Logistics.Shared;

namespace Logistics.Application.Commands;

internal sealed class CreateTenantHandler : RequestHandler<CreateTenantCommand, Result>
{
    private readonly ITenantDatabaseService _tenantDatabase;
    private readonly IMasterUnityOfWork _masterUow;

    public CreateTenantHandler(
        ITenantDatabaseService tenantDatabase,
        IMasterUnityOfWork masterUow)
    {
        _tenantDatabase = tenantDatabase;
        _masterUow = masterUow;
    }

    protected override async Task<Result> HandleValidated(CreateTenantCommand req, CancellationToken cancellationToken)
    {
        var tenantName = req.Name.Trim().ToLower();
        var tenant = new Domain.Entities.Tenant
        {
            Name = tenantName,
            CompanyName = req.CompanyName,
            CompanyAddress = req.CompanyAddress ?? Address.NullAddress,
            ConnectionString = _tenantDatabase.GenerateConnectionString(tenantName)
        };

        var existingTenant = await _masterUow.Repository<Domain.Entities.Tenant>().GetAsync(i => i.Name == tenant.Name);
        
        if (existingTenant is not null)
        {
            return Result.Fail($"Tenant name '{tenant.Name}' is already taken, please chose another name");
        }

        var created = await _tenantDatabase.CreateDatabaseAsync(tenant.ConnectionString);
        if (!created)
        {
            return Result.Fail("Could not create the tenant's database");
        }

        await _masterUow.Repository<Domain.Entities.Tenant>().AddAsync(tenant);
        await _masterUow.SaveChangesAsync();
        return Result.Succeed();
    }
}

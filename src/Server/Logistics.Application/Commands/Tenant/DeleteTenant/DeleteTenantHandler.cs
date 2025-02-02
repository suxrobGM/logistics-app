using Logistics.Application;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;
using Logistics.Shared;

namespace Logistics.Application.Admin.Commands;

internal sealed class DeleteTenantHandler : RequestHandler<DeleteTenantCommand, Result>
{
    private readonly ITenantDatabaseService _tenantDatabase;
    private readonly IMasterUnityOfWork _masterRepository;

    public DeleteTenantHandler(
        ITenantDatabaseService tenantDatabase,
        IMasterUnityOfWork masterRepository)
    {
        _tenantDatabase = tenantDatabase;
        _masterRepository = masterRepository;
    }

    protected override async Task<Result> HandleValidated(DeleteTenantCommand req, CancellationToken cancellationToken)
    {
        var tenant = await _masterRepository.Repository<Domain.Entities.Tenant>().GetByIdAsync(req.Id);

        if (tenant is null)
        {
            return Result.Fail($"Could not find a tenant with ID '{req.Id}'");
        }

        var isDeleted = await _tenantDatabase.DeleteDatabaseAsync(tenant.ConnectionString!);

        if (!isDeleted)
        {
            return Result.Fail("Could not delete the tenant's database");
        }

        _masterRepository.Repository<Domain.Entities.Tenant>().Delete(tenant);
        await _masterRepository.SaveChangesAsync();
        return Result.Succeed();
    }
}

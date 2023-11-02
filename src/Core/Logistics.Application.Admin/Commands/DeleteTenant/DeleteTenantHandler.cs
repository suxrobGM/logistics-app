using Logistics.Application.Core;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;
using Logistics.Shared;

namespace Logistics.Application.Admin.Commands;

internal sealed class DeleteTenantHandler : RequestHandler<DeleteTenantCommand, ResponseResult>
{
    private readonly ITenantDatabaseService _tenantDatabase;
    private readonly IMasterRepository _masterRepository;

    public DeleteTenantHandler(
        ITenantDatabaseService tenantDatabase,
        IMasterRepository masterRepository)
    {
        _tenantDatabase = tenantDatabase;
        _masterRepository = masterRepository;
    }

    protected override async Task<ResponseResult> HandleValidated(DeleteTenantCommand req, CancellationToken cancellationToken)
    {
        var tenant = await _masterRepository.GetAsync<Domain.Entities.Tenant>(req.Id!);

        if (tenant == null)
            return ResponseResult.CreateError("Could not find the tenant");

        var isDeleted = await _tenantDatabase.DeleteDatabaseAsync(tenant.ConnectionString!);

        if (!isDeleted)
            return ResponseResult.CreateError("Could not delete the tenant's database");

        _masterRepository.Delete(tenant);
        await _masterRepository.UnitOfWork.CommitAsync();
        return ResponseResult.CreateSuccess();
    }
}

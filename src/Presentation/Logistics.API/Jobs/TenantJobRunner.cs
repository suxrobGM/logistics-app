using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;

namespace Logistics.API.Jobs;

/// <summary>
/// Shared tenant fan-out for the recurring Hangfire jobs. Runs <paramref name="body"/> once per
/// tenant that has a provisioned database, giving each its own DI scope and its own try/catch so
/// one tenant's failure never aborts the rest of the cycle.
/// </summary>
internal static class TenantJobRunner
{
    /// <remarks>
    /// Feature gates belong inside <paramref name="body"/>, not here: a job may need to run part
    /// of its work for every tenant regardless of the flag (see IftaQuarterCloseJob, where the
    /// quarter snapshot is IFTA-gated but the breadcrumb purge deliberately is not).
    /// </remarks>
    public static async Task ForEachTenantAsync(
        IServiceScopeFactory scopeFactory,
        ILogger logger,
        string operation,
        Func<IServiceScope, Tenant, CancellationToken, Task> body,
        CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var masterUow = scope.ServiceProvider.GetRequiredService<IMasterUnitOfWork>();
        var tenants = await masterUow.Repository<Tenant>().GetListAsync(t => t.ConnectionString != null);

        logger.LogInformation("Starting {Operation} for {TenantCount} tenants", operation, tenants.Count);

        foreach (var tenant in tenants)
        {
            if (ct.IsCancellationRequested)
            {
                break;
            }

            using var tenantScope = scopeFactory.CreateScope();

            try
            {
                await body(tenantScope, tenant, ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during {Operation} for tenant {TenantName}", operation, tenant.Name);
            }
        }

        logger.LogInformation("Completed {Operation} cycle", operation);
    }
}

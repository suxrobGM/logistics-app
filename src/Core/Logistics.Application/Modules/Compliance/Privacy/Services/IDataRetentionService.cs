namespace Logistics.Application.Modules.Compliance.Privacy.Services;

/// <summary>
/// Daily retention sweeper â€” deletes per-tenant entities that have outlived
/// their GDPR retention window (read notifications &gt; 1y, dispatch sessions &gt; 2y).
/// </summary>
public interface IDataRetentionService : IApplicationService
{
    Task PurgeAsync(CancellationToken ct = default);
}

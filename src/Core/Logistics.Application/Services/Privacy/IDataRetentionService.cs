namespace Logistics.Application.Services.Privacy;

/// <summary>
/// Daily retention sweeper — deletes per-tenant entities that have outlived
/// their GDPR retention window (read notifications &gt; 1y, dispatch sessions &gt; 2y).
/// </summary>
public interface IDataRetentionService : IApplicationService
{
    Task PurgeAsync(CancellationToken ct = default);
}

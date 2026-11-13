using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Compliance.Privacy.Services;

/// <summary>Deletes the current tenant's notifications and dispatch sessions past their GDPR retention window.</summary>
public interface IDataRetentionService : IApplicationService
{
    Task PurgeAsync(Tenant tenant, CancellationToken ct = default);
}

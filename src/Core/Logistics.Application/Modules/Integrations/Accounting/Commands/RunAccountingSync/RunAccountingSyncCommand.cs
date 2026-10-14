using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Modules.Integrations.Accounting.Commands;

/// <summary>
///     Run one tenant's accounting sync in the background. Enqueued by "Sync now" and by a retry; a
///     background job has no request tenant, so it carries the tenant id. Not feature-gated here - the
///     sync service checks the feature itself.
/// </summary>
public class RunAccountingSyncCommand : ICommand
{
    public Guid TenantId { get; set; }
}

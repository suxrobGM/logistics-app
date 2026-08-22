using Logistics.Application.Abstractions.Common;

namespace Logistics.Application.Modules.Integrations.Agents.Services;

/// <summary>
/// Display names for the people a transcript attributes rows to. A dispatch conversation is
/// tenant-shared, so a bare user id tells a reader nothing.
/// </summary>
internal interface IAgentSenderDirectory : IApplicationService
{
    Task<string?> GetNameAsync(Guid? userId, CancellationToken ct);

    /// <summary>Names by user id; ids with no employee row are simply absent.</summary>
    Task<Dictionary<Guid, string>> GetNamesAsync(IReadOnlyCollection<Guid> userIds, CancellationToken ct);
}

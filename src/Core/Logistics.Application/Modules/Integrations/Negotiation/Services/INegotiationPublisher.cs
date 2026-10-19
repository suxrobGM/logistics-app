using Logistics.Domain.Entities;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

/// <summary>
/// Pushes a single thread to the tenant's dispatch clients after a state change.
/// </summary>
public interface INegotiationPublisher : IApplicationService
{
    /// <summary>
    /// Fetches the thread's listing and broadcasts the mapped row. The listing is read explicitly
    /// rather than off the navigation because the mapper takes it as an argument precisely so the
    /// lazy-loaded navigation is never touched. Callers that already hold the listing should map and
    /// broadcast directly instead of paying for a second read.
    /// </summary>
    Task PublishAsync(RateNegotiation negotiation, CancellationToken ct);
}

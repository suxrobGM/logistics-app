using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

/// <summary>
/// Maps a batch of negotiations to DTOs.
/// </summary>
public interface INegotiationDtoMapper : IApplicationService
{
    /// <summary>
    /// One query for the whole batch - the listing navigation would lazy-load per row, and the
    /// mapper needs the listing for the lane and the listing rate.
    /// </summary>
    Task<RateNegotiationDto[]> MapAsync(IReadOnlyCollection<RateNegotiation> negotiations, CancellationToken ct);
}

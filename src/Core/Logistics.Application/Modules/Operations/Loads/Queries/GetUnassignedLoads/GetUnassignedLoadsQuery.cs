using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Queries;

/// <summary>
/// Query to get loads that are not assigned to any active trip.
/// Used for attaching existing loads to trips in the trip wizard.
/// </summary>
public sealed class GetUnassignedLoadsQuery : PagedQuery, IQuery<PagedResult<LoadDto>>
{
    /// <summary>Restricts the page to these loads; the total still counts every unassigned load.</summary>
    public Guid[]? LoadIds { get; init; }
}

using Logistics.Domain.Entities;
using Logistics.Mappings;

namespace Logistics.Application.Modules.Operations.Loads;

/// <summary>
/// Builds everything <c>LoadMapper</c> shows for a batch of loads in a fixed number of queries,
/// whatever the batch size. Read off the navigation properties instead, a 50-row page costs 200+
/// lazy SELECTs. Every handler that maps a load uses it, single-row ones included.
/// </summary>
public interface ILoadLookupBuilder : IApplicationService
{
    Task<LoadLookup> BuildAsync(IReadOnlyCollection<Load> loads, CancellationToken ct = default);
}

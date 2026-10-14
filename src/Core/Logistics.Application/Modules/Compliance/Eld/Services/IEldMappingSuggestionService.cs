using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Services;

/// <summary>
/// Suggests driver and vehicle mappings for a provider: its unmapped drivers against the tenant's
/// unmapped drivers (by email, then license number, then name) and its unmapped vehicles against the
/// tenant's unmapped trucks (by VIN, then truck number).
/// </summary>
public interface IEldMappingSuggestionService : IApplicationService
{
    Task<EldMappingSuggestionsDto> GetSuggestionsAsync(EldProviderConfiguration config, CancellationToken ct);
}

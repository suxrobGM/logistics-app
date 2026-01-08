using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ITruckApi
{
    Task<TruckDto?> GetTruckAsync(GetTruckQuery query);
    Task<PagedResponse<TruckDto>?> GetTrucksAsync(SearchableQuery query, bool includeLoads = false);
    Task<bool> CreateTruckAsync(CreateTruckCommand command);
    Task<bool> UpdateTruckAsync(UpdateTruckCommand command);
    Task<bool> DeleteTruckAsync(Guid id);
}

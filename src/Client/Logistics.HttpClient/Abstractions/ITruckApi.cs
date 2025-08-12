using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ITruckApi
{
    Task<Result<TruckDto>> GetTruckAsync(GetTruckQuery query);
    Task<PagedResult<TruckDto>> GetTrucksAsync(SearchableQuery query, bool includeLoads = false);
    Task<Result> CreateTruckAsync(CreateTruckCommand command);
    Task<Result> UpdateTruckAsync(UpdateTruckCommand command);
    Task<Result> DeleteTruckAsync(Guid id);
}

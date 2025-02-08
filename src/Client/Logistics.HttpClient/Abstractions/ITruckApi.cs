using Logistics.Shared.Models;
using Logistics.Shared.Models;
using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ITruckApi
{
    Task<Result<TruckDto>> GetTruckAsync(GetTruckQuery query);
    Task<PagedResult<TruckDto>> GetTrucksAsync(SearchableQuery query, bool includeLoads = false);
    Task<Result> CreateTruckAsync(CreateTruck command);
    Task<Result> UpdateTruckAsync(UpdateTruck command);
    Task<Result> DeleteTruckAsync(string id);
}

using Logistics.Client.Models;
using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.Client.Abstractions;

public interface ITruckApi
{
    Task<ResponseResult<TruckDto>> GetTruckAsync(GetTruckQuery query);
    Task<PagedResponseResult<TruckDto>> GetTrucksAsync(SearchableQuery query, bool includeLoads = false);
    Task<ResponseResult> CreateTruckAsync(CreateTruck command);
    Task<ResponseResult> UpdateTruckAsync(UpdateTruck command);
    Task<ResponseResult> DeleteTruckAsync(string id);
}

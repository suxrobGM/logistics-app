using Logistics.Models;

namespace Logistics.Client.Abstractions;

public interface ITruckApi
{
    Task<ResponseResult<TruckDto>> GetTruckAsync(string id, bool includeLoads = false);
    Task<ResponseResult<TruckDto>> GetTruckByDriverAsync(string userId, bool includeLoads = false, bool includeOnlyActiveLoads = false);
    Task<PagedResponseResult<TruckDto>> GetTrucksAsync(SearchableQuery query, bool includeLoads = false);
    Task<ResponseResult> CreateTruckAsync(CreateTruck truck);
    Task<ResponseResult> UpdateTruckAsync(UpdateTruck truck);
    Task<ResponseResult> DeleteTruckAsync(string id);
}

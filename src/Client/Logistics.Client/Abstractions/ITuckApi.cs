using Logistics.Models;

namespace Logistics.Client.Abstractions;

public interface ITruckApi
{
    Task<ResponseResult<TruckDto>> GetTruckAsync(string id);
    Task<ResponseResult<TruckDto>> GetTruckByDriverAsync(string driverId);
    Task<PagedResponseResult<TruckDto>> GetTrucksAsync(SearchableQuery query, bool includeCargoIds = false);
    Task<ResponseResult> CreateTruckAsync(CreateTruck truck);
    Task<ResponseResult> UpdateTruckAsync(UpdateTruck truck);
    Task<ResponseResult> DeleteTruckAsync(string id);
}

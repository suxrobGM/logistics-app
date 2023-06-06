using Logistics.Client.Models;

namespace Logistics.Client.Abstractions;

public interface ITruckApi
{
    Task<ResponseResult<Truck>> GetTruckAsync(string id);
    Task<ResponseResult<Truck>> GetTruckByDriverAsync(string driverId);
    Task<PagedResponseResult<Truck>> GetTrucksAsync(SearchableQuery query, bool includeCargoIds = false);
    Task<ResponseResult> CreateTruckAsync(CreateTruck truck);
    Task<ResponseResult> UpdateTruckAsync(UpdateTruck truck);
    Task<ResponseResult> DeleteTruckAsync(string id);
}

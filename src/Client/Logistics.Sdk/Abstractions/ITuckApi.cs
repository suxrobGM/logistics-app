namespace Logistics.Sdk;

public interface ITruckApi
{
    Task<ResponseResult<TruckDto>> GetTruckAsync(string id);
    Task<ResponseResult<TruckDto>> GetTruckByDriverAsync(string driverId);
    Task<PagedResponseResult<TruckDto>> GetTrucksAsync(string searchInput = "", int page = 1, int pageSize = 10, bool includeCargoIds = false);
    Task<ResponseResult> CreateTruckAsync(TruckDto truck);
    Task<ResponseResult> UpdateTruckAsync(TruckDto truck);
    Task<ResponseResult> DeleteTruckAsync(string id);
}

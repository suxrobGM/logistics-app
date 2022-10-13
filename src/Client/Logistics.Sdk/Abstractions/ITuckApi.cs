namespace Logistics.Sdk;

public interface ITruckApi
{
    Task<DataResult<TruckDto>> GetTruckAsync(string id);
    Task<DataResult<TruckDto>> GetTruckByDriverAsync(string driverId);
    Task<PagedDataResult<TruckDto>> GetTrucksAsync(string searchInput = "", int page = 1, int pageSize = 10, bool includeCargoIds = false);
    Task<DataResult> CreateTruckAsync(TruckDto truck);
    Task<DataResult> UpdateTruckAsync(TruckDto truck);
    Task<DataResult> DeleteTruckAsync(string id);
}

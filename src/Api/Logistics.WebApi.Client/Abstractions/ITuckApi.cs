namespace Logistics.WebApi.Client;

public interface ITruckApi
{
    Task<TruckDto?> GetTruckAsync(string id);
    Task<TruckDto?> GetTruckByDriverAsync(string driverId);
    Task<PagedDataResult<TruckDto>> GetTrucksAsync(string searchInput = "", int page = 1, int pageSize = 10, bool includeCargoIds = false);
    Task CreateTruckAsync(TruckDto truck);
    Task UpdateTruckAsync(TruckDto truck);
    Task DeleteTruckAsync(string id);
}

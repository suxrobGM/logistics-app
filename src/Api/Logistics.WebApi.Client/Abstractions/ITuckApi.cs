namespace Logistics.WebApi.Client;

public interface ITruckApi
{
    Task<TruckDto> GetTruckAsync(string id);
    Task<PagedDataResult<TruckDto>> GetTrucksAsync(int page = 1, int pageSize = 10);
    Task CreateTruckAsync(TruckDto truck);
    Task UpdateTruckAsync(TruckDto truck);
    Task DeleteTruckAsync(string id);
}

namespace Logistics.WebApi.Client;

public interface ITruckApi
{
    Task CreateTruckAsync(TruckDto truck);
    Task<PagedDataResult<TruckDto>> GetTrucksAsync(int page = 1, int pageSize = 10);
}

namespace Logistics.WebApi.Client;

public interface ICargoApi
{
    Task CreateCargoAsync(CargoDto cargo);
    Task UpdateCargoAsync(CargoDto cargo);
    Task<PagedDataResult<CargoDto>> GetCargoesAsync(int page = 1, int pageSize = 10);
}

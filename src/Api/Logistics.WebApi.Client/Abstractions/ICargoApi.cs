namespace Logistics.WebApi.Client;

public interface ICargoApi
{
    Task<CargoDto> GetCargoAsync(string id);
    Task<PagedDataResult<CargoDto>> GetCargoesAsync(int page = 1, int pageSize = 10);
    Task CreateCargoAsync(CargoDto cargo);
    Task UpdateCargoAsync(CargoDto cargo);
    Task DeleteCargoAsync(string id);
}

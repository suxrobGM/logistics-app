namespace Logistics.WebApi.Client;

public interface ITenantApi
{
    Task<TenantDto?> GetTenantAsync(string id);
    Task<PagedDataResult<TenantDto>> GetTenantsAsync(string searchInput = "", int page = 1, int pageSize = 10);
    Task CreateTenantAsync(TenantDto cargo);
    Task UpdateTenantAsync(TenantDto cargo);
    Task DeleteTenantAsync(string id);
}

namespace Logistics.WebApi.Client;

public interface ITenantApi
{
    Task<string> GetTenantDisplayNameAsync(string identifier);
    Task<TenantDto> GetTenantAsync(string identifier);
    Task<PagedDataResult<TenantDto>> GetTenantsAsync(string searchInput = "", int page = 1, int pageSize = 10);
    Task CreateTenantAsync(TenantDto cargo);
    Task UpdateTenantAsync(TenantDto cargo);
    Task DeleteTenantAsync(string id);
}

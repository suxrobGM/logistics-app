namespace Logistics.WebApi.Client;

public interface ITenantApi
{
    Task<string> GetTenantDisplayNameAsync(string id);
    Task<TenantDto> GetTenantAsync(string identifier);
    Task<PagedDataResult<TenantDto>> GetTenantsAsync(string searchInput = "", int page = 1, int pageSize = 10);
    Task CreateTenantAsync(TenantDto tenant);
    Task UpdateTenantAsync(TenantDto tenant);
    Task DeleteTenantAsync(string id);
}

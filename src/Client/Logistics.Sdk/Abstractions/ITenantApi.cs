namespace Logistics.Sdk;

public interface ITenantApi
{
    Task<DataResult<string>> GetTenantDisplayNameAsync(string id);
    Task<DataResult<TenantDto>> GetTenantAsync(string identifier);
    Task<PagedDataResult<TenantDto>> GetTenantsAsync(string searchInput = "", int page = 1, int pageSize = 10);
    Task<DataResult> CreateTenantAsync(TenantDto tenant);
    Task<DataResult> UpdateTenantAsync(TenantDto tenant);
    Task<DataResult> DeleteTenantAsync(string id);
}

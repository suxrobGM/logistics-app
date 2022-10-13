namespace Logistics.Sdk;

public interface ITenantApi
{
    Task<ResponseResult<string>> GetTenantDisplayNameAsync(string id);
    Task<ResponseResult<TenantDto>> GetTenantAsync(string identifier);
    Task<PagedResponseResult<TenantDto>> GetTenantsAsync(string searchInput = "", int page = 1, int pageSize = 10);
    Task<ResponseResult> CreateTenantAsync(TenantDto tenant);
    Task<ResponseResult> UpdateTenantAsync(TenantDto tenant);
    Task<ResponseResult> DeleteTenantAsync(string id);
}

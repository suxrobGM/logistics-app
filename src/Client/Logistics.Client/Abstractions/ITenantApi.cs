using Logistics.Models;

namespace Logistics.Client.Abstractions;

public interface ITenantApi
{
    Task<ResponseResult<string>> GetTenantDisplayNameAsync(string id);
    Task<ResponseResult<TenantDto>> GetTenantAsync(string identifier);
    Task<PagedResponseResult<TenantDto>> GetTenantsAsync(SearchableRequest request);
    Task<ResponseResult> CreateTenantAsync(CreateTenant tenant);
    Task<ResponseResult> UpdateTenantAsync(UpdateTenant tenant);
    Task<ResponseResult> DeleteTenantAsync(string id);
}

using Logistics.Client.Models;

namespace Logistics.Client.Abstractions;

public interface ITenantApi
{
    Task<ResponseResult<string>> GetTenantDisplayNameAsync(string id);
    Task<ResponseResult<Tenant>> GetTenantAsync(string identifier);
    Task<PagedResponseResult<Tenant>> GetTenantsAsync(SearchableQuery query);
    Task<ResponseResult> CreateTenantAsync(CreateTenant tenant);
    Task<ResponseResult> UpdateTenantAsync(UpdateTenant tenant);
    Task<ResponseResult> DeleteTenantAsync(string id);
}

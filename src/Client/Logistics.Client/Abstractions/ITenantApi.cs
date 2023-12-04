using Logistics.Client.Models;
using Logistics.Shared;
using Logistics.Shared.Models;

namespace Logistics.Client.Abstractions;

public interface ITenantApi
{
    Task<ResponseResult<TenantDto>> GetTenantAsync(string identifier);
    Task<PagedResponseResult<TenantDto>> GetTenantsAsync(SearchableQuery query);
    Task<ResponseResult> CreateTenantAsync(CreateTenant command);
    Task<ResponseResult> UpdateTenantAsync(UpdateTenant command);
    Task<ResponseResult> DeleteTenantAsync(string id);
}

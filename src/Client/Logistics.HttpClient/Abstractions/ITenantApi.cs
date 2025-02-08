using Logistics.Shared.Models;
using Logistics.Shared.Models;
using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ITenantApi
{
    Task<Result<TenantDto>> GetTenantAsync(string identifier);
    Task<PagedResult<TenantDto>> GetTenantsAsync(SearchableQuery query);
    Task<Result> CreateTenantAsync(CreateTenant command);
    Task<Result> UpdateTenantAsync(UpdateTenant command);
    Task<Result> DeleteTenantAsync(string id);
}

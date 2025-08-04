using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ITenantApi
{
    Task<Result<TenantDto>> GetTenantAsync(string identifier);
    Task<PagedResult<TenantDto>> GetTenantsAsync(SearchableQuery query);
    Task<Result> CreateTenantAsync(CreateTenantCommand command);
    Task<Result> UpdateTenantAsync(UpdateTenantCommand command);
    Task<Result> DeleteTenantAsync(Guid id);
}

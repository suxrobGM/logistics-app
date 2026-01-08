using Logistics.Shared.Models;

namespace Logistics.HttpClient.Abstractions;

public interface ITenantApi
{
    Task<TenantDto?> GetTenantAsync(string identifier);
    Task<PagedResponse<TenantDto>?> GetTenantsAsync(SearchableQuery query);
    Task<bool> CreateTenantAsync(CreateTenantCommand command);
    Task<bool> UpdateTenantAsync(UpdateTenantCommand command);
    Task<bool> DeleteTenantAsync(Guid id);
}

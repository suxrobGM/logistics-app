namespace Logistics.DriverApp.Services;

public interface ITenantService
{
    Task<string?> GetTenantIdFromCacheAsync();
    Task SaveTenantIdAsync(string tenantId);
    void ClearCache();
}

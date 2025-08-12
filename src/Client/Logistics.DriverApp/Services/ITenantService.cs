namespace Logistics.DriverApp.Services;

public interface ITenantService
{
    Task<Guid?> GetTenantIdFromCacheAsync();
    Task SaveTenantIdAsync(Guid tenantId);
    void ClearCache();
}

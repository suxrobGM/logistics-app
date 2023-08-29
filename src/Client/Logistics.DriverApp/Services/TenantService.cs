namespace Logistics.DriverApp.Services;

public class TenantService : ITenantService
{
    private const string TENANT_ID_KEY = "tenant_id";
    private string? _currentTenantId;

    public async Task<string?> GetTenantIdFromCacheAsync()
    {
        return await SecureStorage.Default.GetAsync(TENANT_ID_KEY);
    }

    public Task SaveTenantIdAsync(string tenantId)
    {
        if (_currentTenantId == tenantId)
            return Task.CompletedTask;
        
        _currentTenantId = tenantId;
        return SecureStorage.Default.SetAsync(TENANT_ID_KEY, tenantId);
    }
}

using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.Services;

public class TenantService : ITenantService
{
    private const string TENANT_ID_KEY = "tenant_id";
    private readonly IAuthService _authService;
    private string? _currentTenantId;

    public TenantService(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<string?> GetTenantIdFromCacheAsync()
    {
        var cachedTenantId = await SecureStorage.Default.GetAsync(TENANT_ID_KEY);
        var validTenantIds = _authService.User?.TenantIds;

        if (string.IsNullOrEmpty(cachedTenantId) || validTenantIds == null)
        {
            return null;
        }

        return validTenantIds.Contains(cachedTenantId) ? cachedTenantId : null;
    }

    public Task SaveTenantIdAsync(string tenantId)
    {
        if (_currentTenantId == tenantId)
            return Task.CompletedTask;
        
        _currentTenantId = tenantId;
        return SecureStorage.Default.SetAsync(TENANT_ID_KEY, tenantId);
    }

    public void ClearCache()
    {
        SecureStorage.Default.Remove(TENANT_ID_KEY);
    }
}

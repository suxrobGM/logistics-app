using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.Services;

public class TenantService : ITenantService
{
    private const string TenantIdKey = "tenant_id";
    private readonly IAuthService _authService;
    private string? _currentTenantId;

    public TenantService(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<string?> GetTenantIdFromCacheAsync()
    {
        var cachedTenantId = await SecureStorage.Default.GetAsync(TenantIdKey);
        var validTenantId = _authService.User?.TenantId;

        if (string.IsNullOrEmpty(cachedTenantId) || string.IsNullOrEmpty(validTenantId))
        {
            return null;
        }

        return validTenantId == cachedTenantId ? cachedTenantId : null;
    }

    public Task SaveTenantIdAsync(string tenantId)
    {
        if (_currentTenantId == tenantId)
            return Task.CompletedTask;
        
        _currentTenantId = tenantId;
        return SecureStorage.Default.SetAsync(TenantIdKey, tenantId);
    }

    public void ClearCache()
    {
        SecureStorage.Default.Remove(TenantIdKey);
    }
}

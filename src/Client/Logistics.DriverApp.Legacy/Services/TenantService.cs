using Logistics.DriverApp.Services.Authentication;

namespace Logistics.DriverApp.Services;

public class TenantService : ITenantService
{
    private const string TenantIdKey = "tenant_id";
    private readonly IAuthService _authService;
    private Guid? _currentTenantId;

    public TenantService(IAuthService authService)
    {
        _authService = authService;
    }

    public async Task<Guid?> GetTenantIdFromCacheAsync()
    {
        var cachedTenantId = await SecureStorage.Default.GetAsync(TenantIdKey);
        var validTenantId = _authService.User?.TenantId;

        if (!Guid.TryParse(cachedTenantId, out var cachedTenantGuid) || !validTenantId.HasValue)
        {
            return null;
        }

        return validTenantId == cachedTenantGuid ? cachedTenantGuid : null;
    }

    public Task SaveTenantIdAsync(Guid tenantId)
    {
        if (_currentTenantId == tenantId)
            return Task.CompletedTask;

        _currentTenantId = tenantId;
        return SecureStorage.Default.SetAsync(TenantIdKey, tenantId.ToString());
    }

    public void ClearCache()
    {
        SecureStorage.Default.Remove(TenantIdKey);
    }
}

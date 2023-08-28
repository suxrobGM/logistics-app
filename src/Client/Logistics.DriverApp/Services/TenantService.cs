namespace Logistics.DriverApp.Services;

internal static class TenantService
{
    private const string TENANT_ID_KEY = "tenant_id";

    public static string? TenantId { get; private set; }

    public static async Task<string?> GetTenantIdFromCacheAsync()
    {
        return await SecureStorage.Default.GetAsync(TENANT_ID_KEY);
    }

    public static Task SaveTenantIdAsync(string tenantId)
    {
        if (TenantId == tenantId)
            return Task.CompletedTask;
        
        TenantId = tenantId;
        return SecureStorage.Default.SetAsync(TENANT_ID_KEY, tenantId);
    }
}

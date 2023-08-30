namespace Logistics.Domain.Services;

public interface ITenantService
{
    public Task<Tenant> GetTenantAsync();
    public Task<bool> SetTenantAsync(string tenantId);
}

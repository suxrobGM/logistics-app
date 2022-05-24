namespace Logistics.Domain.Services;

public interface ITenantService
{
    public string GetConnectionString();
    public Tenant GetTenant();
}

using Logistics.Domain.Entities;

namespace Logistics.Domain.Services;

public interface ITenantService
{
    public Tenant GetTenant();
    public bool SetTenant(string tenantId);
}

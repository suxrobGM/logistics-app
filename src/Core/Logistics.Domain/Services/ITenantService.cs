using Logistics.Domain.Entities;

namespace Logistics.Domain.Services;

public interface ITenantService
{
    Tenant GetTenant();
    Tenant? SetTenantById(string tenantId);
    void SetTenant(Tenant tenant);
}

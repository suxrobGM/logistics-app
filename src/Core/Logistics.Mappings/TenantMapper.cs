using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class TenantMapper
{
    public static TenantDto ToDto(this Tenant entity, bool includeConnectionString = false)
    {
        return new TenantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CompanyName = entity.CompanyName,
            BillingEmail = entity.BillingEmail,
            CompanyAddress = entity.CompanyAddress.ToDto(),
            ConnectionString = includeConnectionString ? entity.ConnectionString : null,
            Subscription = includeConnectionString ? entity.Subscription?.ToDto() : null
        };
    }
}

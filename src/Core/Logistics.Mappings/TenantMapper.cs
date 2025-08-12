using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class TenantMapper
{
    public static TenantDto ToDto(this Tenant entity, bool includeConnectionString = false, int? employeeCount = null)
    {
        return new TenantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CompanyName = entity.CompanyName,
            BillingEmail = entity.BillingEmail,
            DotNumber = entity.DotNumber,
            CompanyAddress = entity.CompanyAddress,
            ConnectionString = includeConnectionString ? entity.ConnectionString : null,
            StripeCustomerId = entity.StripeCustomerId,
            Subscription = entity.Subscription?.ToDto(),
            EmployeeCount = employeeCount,
        };
    }
}

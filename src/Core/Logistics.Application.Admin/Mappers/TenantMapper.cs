using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Application.Admin;

public static class TenantMapper
{
    public static TenantDto ToDto(this Tenant entity, bool includeConnectionString = false)
    {
        return new TenantDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CompanyName = entity.CompanyName,
            CompanyAddress = entity.CompanyAddress,
            ConnectionString = includeConnectionString ? entity.ConnectionString : null
        };
    }
}

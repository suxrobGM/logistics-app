using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Infrastructure.AI.Tests;

/// <summary>A valid tenant for tools that read the current tenant's settings.</summary>
internal static class TestTenants
{
    public static Tenant Create(DistanceUnit distanceUnit = DistanceUnit.Miles)
    {
        var tenant = new Tenant
        {
            Name = "Test Tenant",
            ConnectionString = "test-connection",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" }
        };
        tenant.Settings.DistanceUnit = distanceUnit;
        return tenant;
    }
}

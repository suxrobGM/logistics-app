using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Tests.TestKit;

/// <summary>A valid tenant. Also compiled into the AI tests through a linked file.</summary>
internal static class TestTenant
{
    public static Tenant Create(
        string? companyName = null,
        string? mcNumber = null,
        DistanceUnit distanceUnit = DistanceUnit.Miles)
    {
        var tenant = new Tenant
        {
            Name = "test",
            CompanyName = companyName,
            McNumber = mcNumber,
            DatabaseName = "test",
            BillingEmail = "billing@test.com",
            CompanyAddress = TestData.Address()
        };
        tenant.Settings.DistanceUnit = distanceUnit;
        return tenant;
    }
}

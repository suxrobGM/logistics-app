using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Tests.TestKit;

internal static class TestTenant
{
    public static Tenant Create(string? companyName = null, string? mcNumber = null) => new()
    {
        Name = "test",
        CompanyName = companyName,
        McNumber = mcNumber,
        ConnectionString = "test",
        BillingEmail = "billing@test.com",
        CompanyAddress = new Address
        {
            Line1 = "1 Test St", City = "Test", State = "TX", ZipCode = "00000", Country = "US"
        }
    };
}

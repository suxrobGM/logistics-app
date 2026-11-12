using System.Linq.Expressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Persistence.Reads;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

public class UserTenantReaderTests
{
    private readonly IMasterUnitOfWork _masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly List<UserTenantAccess> _accesses = [];
    private readonly List<Tenant> _tenants = [];

    public UserTenantReaderTests()
    {
        _masterUow.Repository<UserTenantAccess>().Query().Returns(_ => _accesses.AsQueryable());
        _masterUow.Repository<Tenant>().GetListAsync(
                Arg.Any<Expression<Func<Tenant, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => _tenants.AsQueryable().Where(ci.Arg<Expression<Func<Tenant, bool>>>()).ToList());
    }

    [Fact]
    public async Task ListAsync_HomeAndPortalTenants_ReturnsBothAndSkipsOthers()
    {
        var home = AddTenant();
        var portal = AddTenant();
        AddTenant();
        var user = new User { Email = "user@test.com", FirstName = "Jane", LastName = "Doe", TenantId = home.Id };
        _accesses.Add(new UserTenantAccess { UserId = user.Id, TenantId = portal.Id });
        _accesses.Add(new UserTenantAccess { UserId = Guid.NewGuid(), TenantId = Guid.NewGuid() });

        var tenants = await new UserTenantReader(_masterUow).ListAsync(user, CancellationToken.None);

        Assert.Equal(2, tenants.Count);
        Assert.Contains(home, tenants);
        Assert.Contains(portal, tenants);
    }

    private Tenant AddTenant()
    {
        var tenant = new Tenant
        {
            Name = "test",
            DatabaseName = "test",
            BillingEmail = "billing@test.com",
            CompanyAddress = new Address
            {
                Line1 = "1 Test St",
                City = "Test",
                State = "TX",
                ZipCode = "00000",
                Country = "US"
            }
        };
        _tenants.Add(tenant);
        return tenant;
    }
}

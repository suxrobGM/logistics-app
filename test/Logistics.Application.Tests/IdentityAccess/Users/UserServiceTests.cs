using System.Linq.Expressions;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.IdentityAccess.Users;

public class UserServiceTests
{
    private readonly IMasterUnitOfWork _masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly IServiceScopeFactory _scopeFactory = Substitute.For<IServiceScopeFactory>();
    private readonly Dictionary<Guid, (List<Employee> Employees, List<CustomerUser> CustomerUsers)> _tenantRows = [];
    private readonly List<UserTenantAccess> _accesses = [];
    private readonly List<Tenant> _tenants = [];
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _masterUow.Repository<UserTenantAccess>().GetListAsync(
                Arg.Any<Expression<Func<UserTenantAccess, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => _accesses.AsQueryable().Where(ci.Arg<Expression<Func<UserTenantAccess, bool>>>()).ToList());
        _masterUow.Repository<Tenant>().GetListAsync(
                Arg.Any<Expression<Func<Tenant, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => _tenants.AsQueryable().Where(ci.Arg<Expression<Func<Tenant, bool>>>()).ToList());
        _scopeFactory.CreateScope().Returns(_ => CreateScope());
        _sut = new UserService(_masterUow, _scopeFactory);
    }

    [Fact]
    public async Task SyncTenantProfiles_CopiesProfileIntoHomeTenantAndEmailIntoPortalTenants()
    {
        var home = AddTenant();
        var portal = AddTenant();
        AddTenant();
        var user = new User
        {
            Email = "new@test.com",
            FirstName = "Jane",
            LastName = "Smith",
            PhoneNumber = "555",
            TenantId = home.Id
        };
        _accesses.Add(new UserTenantAccess { UserId = user.Id, TenantId = portal.Id });
        var employee = new Employee
        {
            Id = user.Id,
            Email = "old@test.com",
            FirstName = "Jane",
            LastName = "Doe",
            Salary = Money.Zero("USD")
        };
        _tenantRows[home.Id].Employees.Add(employee);
        var customerUser = new CustomerUser { UserId = user.Id, CustomerId = Guid.NewGuid(), Email = "old@test.com" };
        _tenantRows[portal.Id].CustomerUsers.Add(customerUser);

        await _sut.SyncTenantProfilesAsync(user);

        Assert.Equal(("new@test.com", "Smith", "555"), (employee.Email, employee.LastName, employee.PhoneNumber));
        Assert.Equal("new@test.com", customerUser.Email);
    }

    private Tenant AddTenant()
    {
        var tenant = TestTenant.Create();
        _tenants.Add(tenant);
        _tenantRows[tenant.Id] = ([], []);
        return tenant;
    }

    private IServiceScope CreateScope()
    {
        var tenantUow = Substitute.For<ITenantUnitOfWork>();
        Tenant? current = null;
        tenantUow.When(u => u.SetCurrentTenant(Arg.Any<Tenant>())).Do(ci => current = ci.Arg<Tenant>());
        tenantUow.Repository<Employee>().GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(ci => _tenantRows[current!.Id].Employees.FirstOrDefault(e => e.Id == ci.Arg<Guid>()));
        tenantUow.Repository<CustomerUser>().GetListAsync(
                Arg.Any<Expression<Func<CustomerUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => _tenantRows[current!.Id].CustomerUsers.AsQueryable()
                .Where(ci.Arg<Expression<Func<CustomerUser, bool>>>()).ToList());

        var provider = Substitute.For<IServiceProvider>();
        provider.GetService(typeof(ITenantUnitOfWork)).Returns(tenantUow);
        var scope = Substitute.For<IServiceScope>();
        scope.ServiceProvider.Returns(provider);
        return scope;
    }
}

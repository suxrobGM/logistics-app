using System.Linq.Expressions;
using Logistics.Application.Modules.IdentityAccess.CustomerUsers.Commands;
using Logistics.Application.Modules.IdentityAccess.CustomerUsers.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.IdentityAccess.CustomerUsers;

public class CustomerUserAccessSyncTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IMasterUnitOfWork _masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly ITenantRepository<CustomerUser, Guid> _customerUserRepo =
        Substitute.For<ITenantRepository<CustomerUser, Guid>>();
    private readonly IMasterRepository<UserTenantAccess, Guid> _accessRepo =
        Substitute.For<IMasterRepository<UserTenantAccess, Guid>>();

    private readonly Tenant _tenant = TestTenant.Create();
    private readonly CustomerUser _customerUser;
    private readonly UserTenantAccess _access;

    public CustomerUserAccessSyncTests()
    {
        _customerUser = new CustomerUser { UserId = Guid.NewGuid(), CustomerId = Guid.NewGuid(), Email = "a@b.c" };
        _access = new UserTenantAccess
        {
            UserId = _customerUser.UserId,
            TenantId = _tenant.Id,
            CustomerUserId = _customerUser.Id
        };

        _tenantUow.Repository<CustomerUser>().Returns(_customerUserRepo);
        _tenantUow.GetCurrentTenant().Returns(_tenant);
        _masterUow.Repository<UserTenantAccess>().Returns(_accessRepo);
        _customerUserRepo.GetByIdAsync(_customerUser.Id, Arg.Any<CancellationToken>()).Returns(_customerUser);
        _accessRepo.GetAsync(Arg.Any<Expression<Func<UserTenantAccess, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(_access);
    }

    [Fact]
    public async Task DeleteCustomerUser_WithAccessRow_DeletesBoth()
    {
        var sut = new DeleteCustomerUserHandler(_tenantUow, _masterUow, new CustomerPortalAccess(_tenantUow, _masterUow));

        var result = await sut.Handle(new DeleteCustomerUserCommand { Id = _customerUser.Id }, CancellationToken.None);

        Assert.True(result.IsSuccess);
        _accessRepo.Received(1).Delete(_access);
        _customerUserRepo.Received(1).Delete(_customerUser);
        await _masterUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCustomerUser_Deactivate_DeactivatesAccessRow()
    {
        var sut = new UpdateCustomerUserHandler(_tenantUow, _masterUow, new CustomerPortalAccess(_tenantUow, _masterUow));

        var result = await sut.Handle(
            new UpdateCustomerUserCommand { Id = _customerUser.Id, IsActive = false },
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(_customerUser.IsActive);
        Assert.False(_access.IsActive);
        await _masterUow.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateCustomerUser_ReactivateWhileActiveForAnotherCustomer_Fails()
    {
        _customerUser.IsActive = false;
        var other = new CustomerUser { UserId = _customerUser.UserId, CustomerId = Guid.NewGuid(), Email = "a@b.c" };
        _customerUserRepo.GetAsync(Arg.Any<Expression<Func<CustomerUser, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(ci => new[] { _customerUser, other }.FirstOrDefault(ci.Arg<Expression<Func<CustomerUser, bool>>>().Compile()));
        var sut = new UpdateCustomerUserHandler(_tenantUow, _masterUow, new CustomerPortalAccess(_tenantUow, _masterUow));

        var result = await sut.Handle(
            new UpdateCustomerUserCommand { Id = _customerUser.Id, IsActive = true },
            CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.False(_customerUser.IsActive);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

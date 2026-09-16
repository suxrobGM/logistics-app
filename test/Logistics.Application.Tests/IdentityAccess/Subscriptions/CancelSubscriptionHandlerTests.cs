using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Exceptions;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Identity.Roles;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.IdentityAccess.Subscriptions;

public class CancelSubscriptionHandlerTests
{
    private readonly IMasterUnitOfWork _masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly IStripeSubscriptionService _stripeSubscriptionService = Substitute.For<IStripeSubscriptionService>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ILogger<DeleteSubscriptionHandler> _logger = NullLogger<DeleteSubscriptionHandler>.Instance;

    private readonly IMasterRepository<Subscription, Guid> _subscriptionRepo =
        Substitute.For<IMasterRepository<Subscription, Guid>>();

    private readonly CancelSubscriptionHandler _sut;

    public CancelSubscriptionHandlerTests()
    {
        _masterUow.Repository<Subscription>().Returns(_subscriptionRepo);
        _sut = new CancelSubscriptionHandler(_masterUow, _stripeSubscriptionService, _currentUserService, _logger);
    }

    [Fact]
    public async Task Handle_CrossTenantCaller_ThrowsTenantAccessDeniedException()
    {
        var ownerTenantId = Guid.NewGuid();
        var callerTenantId = Guid.NewGuid();
        var subscription = TestSubscription.Create(ownerTenantId);
        _subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        _currentUserService.GetTenantId().Returns(callerTenantId);

        var command = new CancelSubscriptionCommand
        {
            Id = subscription.Id
        };

        await Assert.ThrowsAsync<TenantAccessDeniedException>(
            () => _sut.Handle(command, CancellationToken.None));

        await _masterUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OwnTenantCaller_Succeeds()
    {
        var tenantId = Guid.NewGuid();
        var subscription = TestSubscription.Create(tenantId);
        _subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        _currentUserService.GetTenantId().Returns(tenantId);

        var command = new CancelSubscriptionCommand
        {
            Id = subscription.Id
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
    }

    [Fact]
    public async Task Handle_PlatformAdmin_SucceedsRegardlessOfTenant()
    {
        var ownerTenantId = Guid.NewGuid();
        var subscription = TestSubscription.Create(ownerTenantId);
        _subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        _currentUserService.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin).Returns(true);

        var command = new CancelSubscriptionCommand
        {
            Id = subscription.Id
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_SubscriptionNotFound_ReturnsFailure()
    {
        _subscriptionRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        var result = await _sut.Handle(
            new CancelSubscriptionCommand { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}

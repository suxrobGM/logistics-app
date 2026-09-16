using System.Linq.Expressions;
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
using NSubstitute.ExceptionExtensions;
using Stripe;
using Xunit;
using Address = Logistics.Domain.Primitives.ValueObjects.Address;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.Application.Tests.IdentityAccess.Subscriptions;

public class RenewSubscriptionHandlerTests
{
    private readonly IMasterUnitOfWork _masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IStripeSubscriptionService _stripeSubscriptionService = Substitute.For<IStripeSubscriptionService>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ILogger<RenewSubscriptionHandler> _logger = NullLogger<RenewSubscriptionHandler>.Instance;

    private readonly IMasterRepository<Subscription, Guid> _subscriptionRepo =
        Substitute.For<IMasterRepository<Subscription, Guid>>();

    private readonly ITenantRepository<Truck, Guid> _truckRepo =
        Substitute.For<ITenantRepository<Truck, Guid>>();

    private readonly RenewSubscriptionHandler _sut;

    public RenewSubscriptionHandlerTests()
    {
        _masterUow.Repository<Subscription>().Returns(_subscriptionRepo);
        _tenantUow.Repository<Truck>().Returns(_truckRepo);
        _truckRepo.CountAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>()).Returns(0);
        _sut = new RenewSubscriptionHandler(_masterUow, _tenantUow, _stripeSubscriptionService, _currentUserService, _logger);
    }

    [Fact]
    public async Task Handle_CrossTenantCaller_ThrowsTenantAccessDeniedException()
    {
        var ownerTenantId = Guid.NewGuid();
        var callerTenantId = Guid.NewGuid();
        var subscription = TestSubscription.Create(ownerTenantId, status: SubscriptionStatus.Cancelled);
        _subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        _currentUserService.GetTenantId().Returns(callerTenantId);

        var command = new RenewSubscriptionCommand
        {
            Id = subscription.Id
        };

        await Assert.ThrowsAsync<TenantAccessDeniedException>(
            () => _sut.Handle(command, CancellationToken.None));

        await _tenantUow.DidNotReceive().SetCurrentTenantByIdAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_OwnTenantCaller_PassesOwnershipCheck()
    {
        var tenantId = Guid.NewGuid();
        var subscription = TestSubscription.Create(tenantId, status: SubscriptionStatus.Cancelled);
        _subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        _currentUserService.GetTenantId().Returns(tenantId);
        _stripeSubscriptionService
            .RenewSubscriptionAsync(Arg.Any<Subscription?>(), Arg.Any<SubscriptionPlan>(), Arg.Any<Tenant>(), Arg.Any<int>())
            .ThrowsAsync(new StripeException("no payment method"));

        var command = new RenewSubscriptionCommand
        {
            Id = subscription.Id
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _tenantUow.Received(1).SetCurrentTenantByIdAsync(tenantId);
    }

    [Fact]
    public async Task Handle_PlatformAdmin_PassesOwnershipCheckRegardlessOfTenant()
    {
        var ownerTenantId = Guid.NewGuid();
        var subscription = TestSubscription.Create(ownerTenantId, status: SubscriptionStatus.Cancelled);
        _subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        _currentUserService.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin).Returns(true);
        _stripeSubscriptionService
            .RenewSubscriptionAsync(Arg.Any<Subscription?>(), Arg.Any<SubscriptionPlan>(), Arg.Any<Tenant>(), Arg.Any<int>())
            .ThrowsAsync(new StripeException("no payment method"));

        var command = new RenewSubscriptionCommand
        {
            Id = subscription.Id
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _tenantUow.Received(1).SetCurrentTenantByIdAsync(ownerTenantId);
    }
}

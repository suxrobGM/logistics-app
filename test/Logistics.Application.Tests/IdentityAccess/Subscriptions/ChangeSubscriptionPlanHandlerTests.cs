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

public class ChangeSubscriptionPlanHandlerTests
{
    private readonly IMasterUnitOfWork _masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IStripeSubscriptionService _stripeSubscriptionService = Substitute.For<IStripeSubscriptionService>();
    private readonly ICurrentUserService _currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ILogger<ChangeSubscriptionPlanHandler> _logger = NullLogger<ChangeSubscriptionPlanHandler>.Instance;

    private readonly IMasterRepository<Subscription, Guid> _subscriptionRepo =
        Substitute.For<IMasterRepository<Subscription, Guid>>();

    private readonly ChangeSubscriptionPlanHandler _sut;

    public ChangeSubscriptionPlanHandlerTests()
    {
        _masterUow.Repository<Subscription>().Returns(_subscriptionRepo);
        _sut = new ChangeSubscriptionPlanHandler(_masterUow, _tenantUow, _stripeSubscriptionService, _currentUserService, _logger);
    }

    [Fact]
    public async Task Handle_CrossTenantCaller_ThrowsTenantAccessDeniedException()
    {
        var ownerTenantId = Guid.NewGuid();
        var callerTenantId = Guid.NewGuid();
        var subscription = TestSubscription.Create(ownerTenantId);
        _subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        _currentUserService.GetTenantId().Returns(callerTenantId);

        var command = new ChangeSubscriptionPlanCommand
        {
            SubscriptionId = subscription.Id,
            NewPlanId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<TenantAccessDeniedException>(
            () => _sut.Handle(command, CancellationToken.None));

        await _masterUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OwnTenantCaller_PassesOwnershipCheck()
    {
        var tenantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var subscription = TestSubscription.Create(tenantId, planId);
        _subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        _currentUserService.GetTenantId().Returns(tenantId);

        var command = new ChangeSubscriptionPlanCommand
        {
            SubscriptionId = subscription.Id,
            NewPlanId = planId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("already on this plan", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_PlatformAdmin_PassesOwnershipCheckRegardlessOfTenant()
    {
        var ownerTenantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var subscription = TestSubscription.Create(ownerTenantId, planId);
        _subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        _currentUserService.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin).Returns(true);

        var command = new ChangeSubscriptionPlanCommand
        {
            SubscriptionId = subscription.Id,
            NewPlanId = planId
        };

        var result = await _sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("already on this plan", result.Error!, StringComparison.OrdinalIgnoreCase);
    }
}

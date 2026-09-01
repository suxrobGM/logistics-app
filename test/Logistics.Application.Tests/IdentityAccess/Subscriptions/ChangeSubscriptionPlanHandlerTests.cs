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
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IStripeSubscriptionService stripeSubscriptionService = Substitute.For<IStripeSubscriptionService>();
    private readonly ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ILogger<ChangeSubscriptionPlanHandler> logger = NullLogger<ChangeSubscriptionPlanHandler>.Instance;

    private readonly IMasterRepository<Subscription, Guid> subscriptionRepo =
        Substitute.For<IMasterRepository<Subscription, Guid>>();

    private readonly ChangeSubscriptionPlanHandler sut;

    public ChangeSubscriptionPlanHandlerTests()
    {
        masterUow.Repository<Subscription>().Returns(subscriptionRepo);
        sut = new ChangeSubscriptionPlanHandler(masterUow, tenantUow, stripeSubscriptionService, currentUserService, logger);
    }

    [Fact]
    public async Task Handle_CrossTenantCaller_ThrowsTenantAccessDeniedException()
    {
        var ownerTenantId = Guid.NewGuid();
        var callerTenantId = Guid.NewGuid();
        var subscription = TestSubscription.Create(ownerTenantId);
        subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        currentUserService.GetTenantId().Returns(callerTenantId);

        var command = new ChangeSubscriptionPlanCommand
        {
            SubscriptionId = subscription.Id,
            NewPlanId = Guid.NewGuid()
        };

        await Assert.ThrowsAsync<TenantAccessDeniedException>(
            () => sut.Handle(command, CancellationToken.None));

        await masterUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OwnTenantCaller_PassesOwnershipCheck()
    {
        var tenantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var subscription = TestSubscription.Create(tenantId, planId);
        subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        currentUserService.GetTenantId().Returns(tenantId);

        var command = new ChangeSubscriptionPlanCommand
        {
            SubscriptionId = subscription.Id,
            NewPlanId = planId
        };

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("already on this plan", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_PlatformAdmin_PassesOwnershipCheckRegardlessOfTenant()
    {
        var ownerTenantId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var subscription = TestSubscription.Create(ownerTenantId, planId);
        subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        currentUserService.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin).Returns(true);

        var command = new ChangeSubscriptionPlanCommand
        {
            SubscriptionId = subscription.Id,
            NewPlanId = planId
        };

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("already on this plan", result.Error!, StringComparison.OrdinalIgnoreCase);
    }
}

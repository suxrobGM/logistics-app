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
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IStripeSubscriptionService stripeSubscriptionService = Substitute.For<IStripeSubscriptionService>();
    private readonly ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ILogger<RenewSubscriptionHandler> logger = NullLogger<RenewSubscriptionHandler>.Instance;

    private readonly IMasterRepository<Subscription, Guid> subscriptionRepo =
        Substitute.For<IMasterRepository<Subscription, Guid>>();

    private readonly ITenantRepository<Truck, Guid> truckRepo =
        Substitute.For<ITenantRepository<Truck, Guid>>();

    private readonly RenewSubscriptionHandler sut;

    public RenewSubscriptionHandlerTests()
    {
        masterUow.Repository<Subscription>().Returns(subscriptionRepo);
        tenantUow.Repository<Truck>().Returns(truckRepo);
        truckRepo.CountAsync(Arg.Any<Expression<Func<Truck, bool>>>(), Arg.Any<CancellationToken>()).Returns(0);
        sut = new RenewSubscriptionHandler(masterUow, tenantUow, stripeSubscriptionService, currentUserService, logger);
    }

    [Fact]
    public async Task Handle_CrossTenantCaller_ThrowsTenantAccessDeniedException()
    {
        var ownerTenantId = Guid.NewGuid();
        var callerTenantId = Guid.NewGuid();
        var subscription = TestSubscription.Create(ownerTenantId, status: SubscriptionStatus.Cancelled);
        subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        currentUserService.GetTenantId().Returns(callerTenantId);

        var command = new RenewSubscriptionCommand
        {
            Id = subscription.Id
        };

        await Assert.ThrowsAsync<TenantAccessDeniedException>(
            () => sut.Handle(command, CancellationToken.None));

        await tenantUow.DidNotReceive().SetCurrentTenantByIdAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_OwnTenantCaller_PassesOwnershipCheck()
    {
        var tenantId = Guid.NewGuid();
        var subscription = TestSubscription.Create(tenantId, status: SubscriptionStatus.Cancelled);
        subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        currentUserService.GetTenantId().Returns(tenantId);
        stripeSubscriptionService
            .RenewSubscriptionAsync(Arg.Any<Subscription?>(), Arg.Any<SubscriptionPlan>(), Arg.Any<Tenant>(), Arg.Any<int>())
            .ThrowsAsync(new StripeException("no payment method"));

        var command = new RenewSubscriptionCommand
        {
            Id = subscription.Id
        };

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await tenantUow.Received(1).SetCurrentTenantByIdAsync(tenantId);
    }

    [Fact]
    public async Task Handle_PlatformAdmin_PassesOwnershipCheckRegardlessOfTenant()
    {
        var ownerTenantId = Guid.NewGuid();
        var subscription = TestSubscription.Create(ownerTenantId, status: SubscriptionStatus.Cancelled);
        subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        currentUserService.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin).Returns(true);
        stripeSubscriptionService
            .RenewSubscriptionAsync(Arg.Any<Subscription?>(), Arg.Any<SubscriptionPlan>(), Arg.Any<Tenant>(), Arg.Any<int>())
            .ThrowsAsync(new StripeException("no payment method"));

        var command = new RenewSubscriptionCommand
        {
            Id = subscription.Id
        };

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await tenantUow.Received(1).SetCurrentTenantByIdAsync(ownerTenantId);
    }
}

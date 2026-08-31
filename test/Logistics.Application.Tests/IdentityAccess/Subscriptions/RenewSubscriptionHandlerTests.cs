using System.Linq.Expressions;
using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Exceptions;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Stripe;
using Xunit;
using Address = Logistics.Domain.Primitives.ValueObjects.Address;
using Subscription = Logistics.Domain.Entities.Subscription;

namespace Logistics.Application.Tests.IdentityAccess.Subscriptions;

/// <summary>
/// Covers the tenant-ownership check added to close finding #6 - RenewSubscription previously
/// had no authorization at all, so any tenant's user could force a renewal (with a real Stripe
/// side effect) on any other tenant's subscription.
/// </summary>
public class RenewSubscriptionHandlerTests
{
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IStripeSubscriptionService stripeSubscriptionService = Substitute.For<IStripeSubscriptionService>();
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
        sut = new RenewSubscriptionHandler(masterUow, tenantUow, stripeSubscriptionService, logger);
    }

    private static Tenant CreateTenant() => new()
    {
        Name = "test-tenant",
        ConnectionString = "test",
        BillingEmail = "test@test.com",
        CompanyAddress = new Address
        {
            Line1 = "123 Main St", City = "NYC", State = "NY", ZipCode = "10001", Country = "US"
        }
    };

    private static SubscriptionPlan CreatePlan() => new()
    {
        Name = "Starter",
        Price = new Money { Amount = 100m, Currency = "USD" },
        PerTruckPrice = new Money { Amount = 10m, Currency = "USD" },
        WeeklyAIBudgetUsd = 50m
    };

    private static Subscription CreateSubscription(Guid tenantId) => new()
    {
        TenantId = tenantId,
        Tenant = CreateTenant(),
        PlanId = Guid.NewGuid(),
        Plan = CreatePlan(),
        Status = SubscriptionStatus.Cancelled
    };

    [Fact]
    public async Task Handle_CrossTenantCaller_ThrowsTenantAccessDeniedException()
    {
        var ownerTenantId = Guid.NewGuid();
        var callerTenantId = Guid.NewGuid();
        var subscription = CreateSubscription(ownerTenantId);
        subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);

        var command = new RenewSubscriptionCommand
        {
            Id = subscription.Id,
            CallerTenantId = callerTenantId,
            IsPlatformAdmin = false
        };

        await Assert.ThrowsAsync<TenantAccessDeniedException>(
            () => sut.Handle(command, CancellationToken.None));

        await tenantUow.DidNotReceive().SetCurrentTenantByIdAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Handle_OwnTenantCaller_PassesOwnershipCheck()
    {
        var tenantId = Guid.NewGuid();
        var subscription = CreateSubscription(tenantId);
        subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        // Fails downstream at the real Stripe call - proves the ownership gate let it through,
        // without needing to fully mock a successful Stripe renewal.
        stripeSubscriptionService
            .RenewSubscriptionAsync(Arg.Any<Subscription?>(), Arg.Any<SubscriptionPlan>(), Arg.Any<Tenant>(), Arg.Any<int>())
            .ThrowsAsync(new StripeException("no payment method"));

        var command = new RenewSubscriptionCommand
        {
            Id = subscription.Id,
            CallerTenantId = tenantId,
            IsPlatformAdmin = false
        };

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await tenantUow.Received(1).SetCurrentTenantByIdAsync(tenantId);
    }

    [Fact]
    public async Task Handle_PlatformAdmin_PassesOwnershipCheckRegardlessOfTenant()
    {
        var ownerTenantId = Guid.NewGuid();
        var subscription = CreateSubscription(ownerTenantId);
        subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        stripeSubscriptionService
            .RenewSubscriptionAsync(Arg.Any<Subscription?>(), Arg.Any<SubscriptionPlan>(), Arg.Any<Tenant>(), Arg.Any<int>())
            .ThrowsAsync(new StripeException("no payment method"));

        var command = new RenewSubscriptionCommand
        {
            Id = subscription.Id,
            CallerTenantId = null,
            IsPlatformAdmin = true
        };

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await tenantUow.Received(1).SetCurrentTenantByIdAsync(ownerTenantId);
    }
}

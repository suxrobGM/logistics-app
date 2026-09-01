using Logistics.Application.Abstractions.CurrentUser;
using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Application.Modules.IdentityAccess.Subscriptions.Commands;
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

/// <summary>
/// Covers the tenant-ownership check added to close finding #6 (subscription/billing
/// manipulation) - CancelSubscription previously had no authorization at all beyond "is logged
/// in", so any tenant's user could cancel any other tenant's paid subscription.
/// </summary>
public class CancelSubscriptionHandlerTests
{
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly IStripeSubscriptionService stripeSubscriptionService = Substitute.For<IStripeSubscriptionService>();
    private readonly ICurrentUserService currentUserService = Substitute.For<ICurrentUserService>();
    private readonly ILogger<DeleteSubscriptionHandler> logger = NullLogger<DeleteSubscriptionHandler>.Instance;

    private readonly IMasterRepository<Subscription, Guid> subscriptionRepo =
        Substitute.For<IMasterRepository<Subscription, Guid>>();

    private readonly CancelSubscriptionHandler sut;

    public CancelSubscriptionHandlerTests()
    {
        masterUow.Repository<Subscription>().Returns(subscriptionRepo);
        sut = new CancelSubscriptionHandler(masterUow, stripeSubscriptionService, currentUserService, logger);
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

    private static Subscription CreateSubscription(Guid tenantId, string? stripeSubscriptionId = null) => new()
    {
        TenantId = tenantId,
        Tenant = CreateTenant(),
        PlanId = Guid.NewGuid(),
        Plan = CreatePlan(),
        Status = SubscriptionStatus.Active,
        StripeSubscriptionId = stripeSubscriptionId
    };

    [Fact]
    public async Task Handle_CrossTenantCaller_ThrowsTenantAccessDeniedException()
    {
        var ownerTenantId = Guid.NewGuid();
        var callerTenantId = Guid.NewGuid();
        var subscription = CreateSubscription(ownerTenantId);
        subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        currentUserService.GetTenantId().Returns(callerTenantId);

        var command = new CancelSubscriptionCommand
        {
            Id = subscription.Id
        };

        await Assert.ThrowsAsync<TenantAccessDeniedException>(
            () => sut.Handle(command, CancellationToken.None));

        await masterUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OwnTenantCaller_Succeeds()
    {
        var tenantId = Guid.NewGuid();
        var subscription = CreateSubscription(tenantId);
        subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        currentUserService.GetTenantId().Returns(tenantId);

        var command = new CancelSubscriptionCommand
        {
            Id = subscription.Id
        };

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(SubscriptionStatus.Cancelled, subscription.Status);
    }

    [Fact]
    public async Task Handle_PlatformAdmin_SucceedsRegardlessOfTenant()
    {
        var ownerTenantId = Guid.NewGuid();
        var subscription = CreateSubscription(ownerTenantId);
        subscriptionRepo.GetByIdAsync(subscription.Id, Arg.Any<CancellationToken>()).Returns(subscription);
        currentUserService.IsInRole(AppRoles.SuperAdmin, AppRoles.Admin).Returns(true);

        var command = new CancelSubscriptionCommand
        {
            Id = subscription.Id
        };

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_SubscriptionNotFound_ReturnsFailure()
    {
        subscriptionRepo.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Subscription?)null);

        var result = await sut.Handle(
            new CancelSubscriptionCommand { Id = Guid.NewGuid() }, CancellationToken.None);

        Assert.False(result.IsSuccess);
    }
}

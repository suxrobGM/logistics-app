using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Persistence.Services.AIDispatch;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

public class AIQuotaServiceTests
{
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();

    private readonly IMasterRepository<SubscriptionPlan, Guid> planRepo =
        Substitute.For<IMasterRepository<SubscriptionPlan, Guid>>();

    private readonly ITenantRepository<AgentSession, Guid> sessionRepo =
        Substitute.For<ITenantRepository<AgentSession, Guid>>();

    private readonly AIQuotaService sut;
    private readonly IMasterRepository<Tenant, Guid> tenantRepo = Substitute.For<IMasterRepository<Tenant, Guid>>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();

    public AIQuotaServiceTests()
    {
        masterUow.Repository<Tenant>().Returns(tenantRepo);
        masterUow.Repository<SubscriptionPlan>().Returns(planRepo);
        tenantUow.Repository<AgentSession>().Returns(sessionRepo);
        sut = new AIQuotaService(masterUow, tenantUow);
    }

    private const decimal EnterpriseBudget = 75m;

    private void SetupTenantWithPlan(Guid tenantId, decimal weeklyBudgetUsd, bool blockOverage = false)
    {
        var planId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "Test",
            ConnectionString = "test",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" },
            IsSubscriptionRequired = true,
            Subscription = new Subscription { PlanId = planId, TenantId = tenantId, Tenant = null!, Plan = null! },
            Settings = new() { BlockAIOverage = blockOverage }
        };

        var plan = new SubscriptionPlan
        {
            Id = planId,
            Name = "Test Plan",
            Tier = PlanTier.Starter,
            Price = new() { Amount = 29m, Currency = "USD" },
            PerTruckPrice = new() { Amount = 12m, Currency = "USD" },
            WeeklyAIBudgetUsd = weeklyBudgetUsd
        };

        tenantRepo.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);
        planRepo.GetByIdAsync(planId, Arg.Any<CancellationToken>()).Returns(plan);
        SetupPlanCatalogue(plan);
    }

    /// <summary>The given plans plus the Enterprise one the fallback resolves through.</summary>
    private void SetupPlanCatalogue(params SubscriptionPlan[] plans)
    {
        List<SubscriptionPlan> catalogue =
        [
            ..plans,
            new SubscriptionPlan
            {
                Id = Guid.NewGuid(),
                Name = "Enterprise",
                Tier = PlanTier.Enterprise,
                Price = new() { Amount = 169m, Currency = "USD" },
                PerTruckPrice = new() { Amount = 6m, Currency = "USD" },
                WeeklyAIBudgetUsd = EnterpriseBudget
            }
        ];

        planRepo.GetListAsync(ct: Arg.Any<CancellationToken>()).Returns(catalogue);
    }

    private void SetupSessions(params AgentSession[] sessions)
    {
        var mock = sessions.ToList().BuildMock();
        sessionRepo.Query().Returns(mock);
    }

    private static AgentSession CreateCompletedSessionAt(
        DateTime startedAt, decimal costUsd, bool isOverage = false)
    {
        var session = new AgentSession { StartedAt = startedAt, EstimatedCostUsd = costUsd, IsOverage = isOverage };
        session.Complete("done");
        return session;
    }

    #region Budget from plan

    [Theory]
    [InlineData(5)]
    [InlineData(25)]
    [InlineData(75.50)]
    public async Task GetQuotaStatus_ReturnsCorrectBudgetFromPlan(decimal budget)
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, budget);
        SetupSessions();

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.Equal(budget, status.WeeklyBudgetUsd);
    }

    #endregion

    #region Tenants without a budget of their own

    private void SetupTenantWithoutSubscription(Guid tenantId, bool blockOverage = false)
    {
        tenantRepo.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(new Tenant
        {
            Id = tenantId,
            Name = "Free Tenant",
            ConnectionString = "test",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" },
            IsSubscriptionRequired = false,
            Settings = new() { BlockAIOverage = blockOverage }
        });
        SetupPlanCatalogue();
    }

    [Fact]
    public async Task GetQuotaStatus_NonSubscriptionTenant_MetersAgainstEnterpriseBudget()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithoutSubscription(tenantId);
        SetupSessions();

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.False(status.IsOverQuota);
        Assert.Equal(EnterpriseBudget, status.WeeklyBudgetUsd);
    }

    [Fact]
    public async Task GetQuotaStatus_NonSubscriptionTenant_ReportsSpend()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithoutSubscription(tenantId);

        var now = DateTime.UtcNow;
        SetupSessions(
            CreateCompletedSessionAt(now, 0.75m),
            CreateCompletedSessionAt(now, 0.25m));

        var status = await sut.GetQuotaStatusAsync(tenantId);

        // Otherwise the usage panel reads as "never used AI".
        Assert.Equal(1m, status.SpentThisWeekUsd);
        Assert.NotNull(status.ResetsAt);
    }

    [Fact]
    public async Task GetQuotaStatus_TenantNotFound_ReturnsEmptyStatus()
    {
        var tenantId = Guid.NewGuid();
        tenantRepo.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns((Tenant?)null);

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.False(status.IsOverQuota);
        Assert.Equal(0m, status.SpentThisWeekUsd);
    }

    #endregion

    #region Cost counting

    [Fact]
    public async Task GetQuotaStatus_CountsCostFromEveryStatus()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 10m);

        var now = DateTime.UtcNow;
        var completed = new AgentSession { StartedAt = now, EstimatedCostUsd = 0.50m };
        completed.Complete("done");
        var running = new AgentSession { StartedAt = now, EstimatedCostUsd = 0m };
        var failed = new AgentSession { StartedAt = now, EstimatedCostUsd = 0.25m };
        failed.Fail("error");
        var cancelled = new AgentSession { StartedAt = now, EstimatedCostUsd = 0.10m };
        cancelled.Cancel();

        SetupSessions(completed, running, failed, cancelled);

        var status = await sut.GetQuotaStatusAsync(tenantId);

        // Failed and cancelled sessions burned real tokens, so their cost counts too.
        Assert.Equal(0.85m, status.SpentThisWeekUsd);
    }

    [Fact]
    public async Task GetQuotaStatus_ZeroCostSessions_AddNothing()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 10m);

        var now = DateTime.UtcNow;
        // A session that died before the loop (or an in-flight one) carries zero cost.
        var preLoopFailure = new AgentSession { StartedAt = now, EstimatedCostUsd = 0m };
        preLoopFailure.Fail("setup error");
        var running = new AgentSession { StartedAt = now, EstimatedCostUsd = 0m };

        SetupSessions(preLoopFailure, running);

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.Equal(0m, status.SpentThisWeekUsd);
        Assert.False(status.IsOverQuota);
    }

    [Fact]
    public async Task GetQuotaStatus_ExcludesSessionsFromPreviousWeek()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 10m);

        var now = DateTime.UtcNow;
        var thisWeek = CreateCompletedSessionAt(now, 1m);
        var lastWeek = CreateCompletedSessionAt(now.AddDays(-14), 5m);

        SetupSessions(thisWeek, lastWeek);

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.Equal(1m, status.SpentThisWeekUsd);
    }

    #endregion

    #region Over budget detection

    [Fact]
    public async Task GetQuotaStatus_UnderBudget_IsOverQuotaFalse()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 10m);
        var now = DateTime.UtcNow;
        SetupSessions(
            CreateCompletedSessionAt(now, 1m),
            CreateCompletedSessionAt(now, 0.50m)
        );

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.False(status.IsOverQuota);
        Assert.Equal(1.50m, status.SpentThisWeekUsd);
    }

    [Fact]
    public async Task GetQuotaStatus_AtExactBudget_IsOverQuotaTrue()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 3m);

        var now = DateTime.UtcNow;
        SetupSessions(
            CreateCompletedSessionAt(now, 1m),
            CreateCompletedSessionAt(now, 1m),
            CreateCompletedSessionAt(now, 1m)
        );

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.True(status.IsOverQuota);
        Assert.Equal(3m, status.SpentThisWeekUsd);
    }

    [Fact]
    public async Task GetQuotaStatus_OverBudget_IsOverQuotaTrue()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 2m);

        var now = DateTime.UtcNow;
        SetupSessions(
            CreateCompletedSessionAt(now, 1.75m),
            CreateCompletedSessionAt(now, 1.75m)
        );

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.True(status.IsOverQuota);
        Assert.Equal(3.50m, status.SpentThisWeekUsd);
    }

    #endregion

    #region Overage charges

    [Fact]
    public async Task GetQuotaStatus_OverageCharges_SumBilledUnitsPerCompletedOverageSession()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 1m);

        var now = DateTime.UtcNow;
        var failedOverage = new AgentSession { StartedAt = now, EstimatedCostUsd = 0.50m, IsOverage = true };
        failedOverage.Fail("error");

        SetupSessions(
            CreateCompletedSessionAt(now, 0.04m, isOverage: true),  // 0.12 marked up -> 2 units
            CreateCompletedSessionAt(now, 0.01m, isOverage: true),  // 0.03 -> min-rounds to 1 unit
            failedOverage,                                          // failed: never billed
            CreateCompletedSessionAt(now, 0.90m),                   // completed but not overage
            CreateCompletedSessionAt(now.AddDays(-14), 0.30m, isOverage: true)); // last week

        var status = await sut.GetQuotaStatusAsync(tenantId);

        // 3 units x $0.10 - per-session rounding, matching what Stripe actually invoices.
        Assert.Equal(0.30m, status.OverageChargesUsd);
    }

    [Fact]
    public async Task GetQuotaStatus_NoOverageSessions_ChargesAreZero()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 10m);
        SetupSessions(CreateCompletedSessionAt(DateTime.UtcNow, 1m));

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.Equal(0m, status.OverageChargesUsd);
    }

    #endregion

    #region Overage blocking

    [Fact]
    public async Task GetQuotaStatus_BlockOverageAndOverBudget_OverageBlockedTrue()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 2m, blockOverage: true);
        SetupSessions(CreateCompletedSessionAt(DateTime.UtcNow, 3m));

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.True(status.OverageBlocked);
    }

    [Fact]
    public async Task GetQuotaStatus_BlockOverageUnderBudget_OverageBlockedFalse()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 10m, blockOverage: true);
        SetupSessions(CreateCompletedSessionAt(DateTime.UtcNow, 1m));

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.False(status.OverageBlocked);
    }

    [Fact]
    public async Task GetQuotaStatus_OverBudgetWithoutBlock_OverageBlockedFalse()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 2m);
        SetupSessions(CreateCompletedSessionAt(DateTime.UtcNow, 3m));

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.True(status.IsOverQuota);
        Assert.False(status.OverageBlocked);
    }

    [Fact]
    public async Task GetQuotaStatus_NonSubscriptionTenantWithBlock_BlocksOnTheFallbackBudget()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithoutSubscription(tenantId, blockOverage: true);
        SetupSessions(CreateCompletedSessionAt(DateTime.UtcNow, EnterpriseBudget + 1m));

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.True(status.IsOverQuota);
        Assert.True(status.OverageBlocked);
    }

    #endregion
}

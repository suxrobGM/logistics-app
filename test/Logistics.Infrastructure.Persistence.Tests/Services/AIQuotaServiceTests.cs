using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
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

    private void SetupTenantWithPlan(Guid tenantId, decimal? weeklyBudgetUsd)
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
            Subscription = new Subscription { PlanId = planId, TenantId = tenantId, Tenant = null!, Plan = null! }
        };

        tenantRepo.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);
        planRepo.GetByIdAsync(planId, Arg.Any<CancellationToken>())
            .Returns(new SubscriptionPlan
            {
                Id = planId,
                Name = "Test Plan",
                Price = new() { Amount = 29m, Currency = "USD" },
                PerTruckPrice = new() { Amount = 12m, Currency = "USD" },
                WeeklyAIBudgetUsd = weeklyBudgetUsd
            });
    }

    private void SetupSessions(params AgentSession[] sessions)
    {
        var mock = sessions.ToList().BuildMock();
        sessionRepo.Query().Returns(mock);
    }

    private static AgentSession CreateCompletedSessionAt(DateTime startedAt, decimal costUsd)
    {
        var session = new AgentSession { StartedAt = startedAt, EstimatedCostUsd = costUsd };
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

    #region Non-subscription tenants

    [Fact]
    public async Task GetQuotaStatus_NonSubscriptionTenant_ReturnsUnlimited()
    {
        var tenantId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "Free Tenant",
            ConnectionString = "test",
            BillingEmail = "test@test.com",
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" },
            IsSubscriptionRequired = false
        };
        tenantRepo.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.False(status.IsOverQuota);
        Assert.Equal(0m, status.WeeklyBudgetUsd);
    }

    [Fact]
    public async Task GetQuotaStatus_TenantNotFound_ReturnsUnlimited()
    {
        var tenantId = Guid.NewGuid();
        tenantRepo.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns((Tenant?)null);

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.False(status.IsOverQuota);
    }

    [Fact]
    public async Task GetQuotaStatus_NullBudgetOnPlan_ReturnsUnlimited()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, null);

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.False(status.IsOverQuota);
        Assert.Equal(0m, status.WeeklyBudgetUsd);
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
}

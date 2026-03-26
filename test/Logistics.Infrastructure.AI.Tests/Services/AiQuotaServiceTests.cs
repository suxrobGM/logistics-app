using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Services;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Services;

public class AiQuotaServiceTests
{
    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();

    private readonly IMasterRepository<SubscriptionPlan, Guid> planRepo =
        Substitute.For<IMasterRepository<SubscriptionPlan, Guid>>();

    private readonly ITenantRepository<DispatchSession, Guid> sessionRepo =
        Substitute.For<ITenantRepository<DispatchSession, Guid>>();

    private readonly AiQuotaService sut;
    private readonly IMasterRepository<Tenant, Guid> tenantRepo = Substitute.For<IMasterRepository<Tenant, Guid>>();
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();

    public AiQuotaServiceTests()
    {
        masterUow.Repository<Tenant>().Returns(tenantRepo);
        masterUow.Repository<SubscriptionPlan>().Returns(planRepo);
        tenantUow.Repository<DispatchSession>().Returns(sessionRepo);
        sut = new AiQuotaService(masterUow, tenantUow);
    }

    private void SetupTenantWithPlan(Guid tenantId, int? weeklyQuota)
    {
        var planId = Guid.NewGuid();
        var tenant = new Tenant
        {
            Id = tenantId,
            Name = "Test",
            ConnectionString = "test",
            BillingEmail = "test@test.com",
            IsSubscriptionRequired = true,
            Subscription = new Subscription { PlanId = planId, TenantId = tenantId, Tenant = null!, Plan = null! }
        };

        tenantRepo.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);
        planRepo.GetByIdAsync(planId, Arg.Any<CancellationToken>())
            .Returns(new SubscriptionPlan
            {
                Id = planId,
                Name = "Test Plan",
                Price = 29m,
                PerTruckPrice = 12m,
                WeeklyAiSessionQuota = weeklyQuota
            });
    }

    private void SetupSessions(params DispatchSession[] sessions)
    {
        var mock = sessions.ToList().BuildMock();
        sessionRepo.Query().Returns(mock);
    }

    #region Quota from plan

    [Theory]
    [InlineData(25)]
    [InlineData(100)]
    [InlineData(250)]
    public async Task GetQuotaStatus_ReturnsCorrectQuotaFromPlan(int quota)
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, quota);
        SetupSessions();

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.Equal(quota, status.WeeklyQuota);
    }

    #endregion

    private static DispatchSession CreateSessionAt(DateTime startedAt)
    {
        return new DispatchSession
        {
            StartedAt = startedAt
            // Default status is Running, which counts toward quota
        };
    }

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
            IsSubscriptionRequired = false
        };
        tenantRepo.GetByIdAsync(tenantId, Arg.Any<CancellationToken>()).Returns(tenant);

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.False(status.IsOverQuota);
        Assert.Equal(0, status.WeeklyQuota);
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
    public async Task GetQuotaStatus_NullQuotaOnPlan_ReturnsUnlimited()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, null);

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.False(status.IsOverQuota);
        Assert.Equal(0, status.WeeklyQuota);
    }

    #endregion

    #region Session counting

    [Fact]
    public async Task GetQuotaStatus_CountsOnlyCompletedAndRunningSessions()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 25);

        var now = DateTime.UtcNow;
        var completed = CreateSessionAt(now);
        completed.Complete("done");
        var running = CreateSessionAt(now); // default status is Running
        var failed = CreateSessionAt(now);
        failed.Fail("error");
        var cancelled = CreateSessionAt(now);
        cancelled.Cancel();

        SetupSessions(completed, running, failed, cancelled);

        var status = await sut.GetQuotaStatusAsync(tenantId);

        // Only Completed + Running count
        Assert.Equal(2, status.UsedThisWeek);
        Assert.Equal(23, status.Remaining);
    }

    [Fact]
    public async Task GetQuotaStatus_ExcludesSessionsFromPreviousWeek()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 25);

        var now = DateTime.UtcNow;
        var thisWeek = CreateSessionAt(now);
        thisWeek.Complete("done");
        var lastWeek = CreateSessionAt(now.AddDays(-14));
        lastWeek.Complete("done");

        SetupSessions(thisWeek, lastWeek);

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.Equal(1, status.UsedThisWeek);
    }

    #endregion

    #region Over quota detection

    [Fact]
    public async Task GetQuotaStatus_UnderQuota_IsOverQuotaFalse()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 25);
        SetupSessions(
            CreateSessionAt(DateTime.UtcNow),
            CreateSessionAt(DateTime.UtcNow)
        );

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.False(status.IsOverQuota);
        Assert.Equal(23, status.Remaining);
    }

    [Fact]
    public async Task GetQuotaStatus_AtQuota_IsOverQuotaTrue()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 3);

        var now = DateTime.UtcNow;
        SetupSessions(
            CreateSessionAt(now),
            CreateSessionAt(now),
            CreateSessionAt(now)
        );

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.True(status.IsOverQuota);
        Assert.Equal(0, status.Remaining);
    }

    [Fact]
    public async Task GetQuotaStatus_OverQuota_RemainingIsZero()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 2);

        var now = DateTime.UtcNow;
        SetupSessions(
            CreateSessionAt(now),
            CreateSessionAt(now),
            CreateSessionAt(now),
            CreateSessionAt(now),
            CreateSessionAt(now)
        );

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.True(status.IsOverQuota);
        Assert.Equal(0, status.Remaining);
        Assert.Equal(5, status.UsedThisWeek);
    }

    #endregion
}

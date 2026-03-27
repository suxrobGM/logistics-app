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

    private static DispatchSession CreateCompletedSessionAt(DateTime startedAt)
    {
        var session = new DispatchSession { StartedAt = startedAt };
        session.Complete("done");
        return session;
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
            CompanyAddress = new() { Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US" },
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
    public async Task GetQuotaStatus_CountsOnlyCompletedSessions()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 25);

        var now = DateTime.UtcNow;
        var completed = new DispatchSession { StartedAt = now };
        completed.Complete("done");
        var running = new DispatchSession { StartedAt = now };
        var failed = new DispatchSession { StartedAt = now };
        failed.Fail("error");
        var cancelled = new DispatchSession { StartedAt = now };
        cancelled.Cancel();

        SetupSessions(completed, running, failed, cancelled);

        var status = await sut.GetQuotaStatusAsync(tenantId);

        // Only Completed sessions count toward quota
        Assert.Equal(1, status.UsedThisWeek);
        Assert.Equal(24, status.Remaining);
    }

    [Fact]
    public async Task GetQuotaStatus_ExcludesSessionsFromPreviousWeek()
    {
        var tenantId = Guid.NewGuid();
        SetupTenantWithPlan(tenantId, 25);

        var now = DateTime.UtcNow;
        var thisWeek = CreateCompletedSessionAt(now);
        var lastWeek = CreateCompletedSessionAt(now.AddDays(-14));

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
        var now = DateTime.UtcNow;
        SetupSessions(
            CreateCompletedSessionAt(now),
            CreateCompletedSessionAt(now)
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
            CreateCompletedSessionAt(now),
            CreateCompletedSessionAt(now),
            CreateCompletedSessionAt(now)
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
            CreateCompletedSessionAt(now),
            CreateCompletedSessionAt(now),
            CreateCompletedSessionAt(now),
            CreateCompletedSessionAt(now),
            CreateCompletedSessionAt(now)
        );

        var status = await sut.GetQuotaStatusAsync(tenantId);

        Assert.True(status.IsOverQuota);
        Assert.Equal(0, status.Remaining);
        Assert.Equal(5, status.UsedThisWeek);
    }

    #endregion
}

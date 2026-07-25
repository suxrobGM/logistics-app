using System.Linq.Expressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Specifications;
using Logistics.Infrastructure.Persistence.Services.Feature;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

/// <summary>
/// The memo is what keeps a request that gates on several features from paying 3-5 master-DB round
/// trips per check. It is invisible in behaviour, so these assert the round-trip counts directly.
/// </summary>
public class FeatureServiceCachingTests
{
    private readonly IMasterRepository<TenantFeatureConfig, Guid> configRepo =
        Substitute.For<IMasterRepository<TenantFeatureConfig, Guid>>();

    private readonly IMasterRepository<DefaultFeatureConfig, Guid> defaultRepo =
        Substitute.For<IMasterRepository<DefaultFeatureConfig, Guid>>();

    private readonly IMasterUnitOfWork masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly FeatureService sut;
    private readonly Guid tenantId = Guid.NewGuid();
    private readonly IMasterRepository<Tenant, Guid> tenantRepo = Substitute.For<IMasterRepository<Tenant, Guid>>();

    public FeatureServiceCachingTests()
    {
        masterUow.Repository<TenantFeatureConfig>().Returns(configRepo);
        masterUow.Repository<DefaultFeatureConfig>().Returns(defaultRepo);
        masterUow.Repository<Tenant>().Returns(tenantRepo);

        configRepo.GetListAsync(Arg.Any<Expression<Func<TenantFeatureConfig, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(_ => new List<TenantFeatureConfig>());
        defaultRepo.GetListAsync(Arg.Any<ISpecification<DefaultFeatureConfig>?>(), Arg.Any<CancellationToken>())
            .Returns(_ => new List<DefaultFeatureConfig>());

        // Non-subscription tenant: plan gating is skipped, so all features are allowed.
        tenantRepo.GetByIdAsync(tenantId, Arg.Any<CancellationToken>())
            .Returns(new Tenant
            {
                Id = tenantId,
                Name = "Test",
                ConnectionString = "test",
                BillingEmail = "test@test.com",
                CompanyAddress = new()
                {
                    Line1 = "123 Test St", City = "Test", State = "TX", ZipCode = "12345", Country = "US"
                },
                IsSubscriptionRequired = false
            });

        sut = new FeatureService(masterUow);
    }

    private int ConfigQueryCount => configRepo.ReceivedCalls().Count(c => c.GetMethodInfo().Name == "GetListAsync");
    private int DefaultQueryCount => defaultRepo.ReceivedCalls().Count(c => c.GetMethodInfo().Name == "GetListAsync");
    private int TenantQueryCount => tenantRepo.ReceivedCalls().Count(c => c.GetMethodInfo().Name == "GetByIdAsync");

    [Fact]
    public async Task IsFeatureEnabled_SameFeatureTwice_QueriesMasterOnce()
    {
        await sut.IsFeatureEnabledAsync(tenantId, TenantFeature.AgenticDispatch);
        await sut.IsFeatureEnabledAsync(tenantId, TenantFeature.AgenticDispatch);

        Assert.Equal(1, ConfigQueryCount);
        Assert.Equal(1, DefaultQueryCount);
        Assert.Equal(1, TenantQueryCount);
    }

    [Fact]
    public async Task IsFeatureEnabled_DifferentFeatures_StillQueriesMasterOnce()
    {
        foreach (var feature in Enum.GetValues<TenantFeature>())
        {
            await sut.IsFeatureEnabledAsync(tenantId, feature);
        }

        Assert.Equal(1, ConfigQueryCount);
        Assert.Equal(1, DefaultQueryCount);
        Assert.Equal(1, TenantQueryCount);
    }

    [Fact]
    public async Task IsFeatureEnabled_DifferentTenants_QueriesPerTenantButSharesDefaults()
    {
        var otherTenantId = Guid.NewGuid();

        await sut.IsFeatureEnabledAsync(tenantId, TenantFeature.AgenticDispatch);
        await sut.IsFeatureEnabledAsync(otherTenantId, TenantFeature.AgenticDispatch);

        Assert.Equal(2, ConfigQueryCount);
        Assert.Equal(2, TenantQueryCount);
        Assert.Equal(1, DefaultQueryCount); // defaults are tenant-independent
    }

    [Fact]
    public async Task InitializeFeaturesForTenant_EvictsTenantConfigs_SoNextReadSeesTheNewRows()
    {
        await sut.IsFeatureEnabledAsync(tenantId, TenantFeature.AgenticDispatch);
        Assert.Equal(1, ConfigQueryCount);

        await sut.InitializeFeaturesForTenantAsync(tenantId);
        await sut.IsFeatureEnabledAsync(tenantId, TenantFeature.AgenticDispatch);

        // Initialize reused the cached list, then evicted it; the read after re-queries.
        Assert.Equal(2, ConfigQueryCount);
    }

    [Fact]
    public async Task GetEnabledFeatures_AfterIsFeatureEnabled_ReusesTheSameCache()
    {
        await sut.IsFeatureEnabledAsync(tenantId, TenantFeature.AgenticDispatch);
        await sut.GetEnabledFeaturesAsync(tenantId);
        await sut.GetAllFeatureStatusAsync(tenantId);

        Assert.Equal(1, ConfigQueryCount);
        Assert.Equal(1, DefaultQueryCount);
        Assert.Equal(1, TenantQueryCount);
    }
}

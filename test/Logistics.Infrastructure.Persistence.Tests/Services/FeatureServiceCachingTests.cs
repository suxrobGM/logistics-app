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
    private readonly IMasterRepository<TenantFeatureConfig, Guid> _configRepo =
        Substitute.For<IMasterRepository<TenantFeatureConfig, Guid>>();

    private readonly IMasterRepository<DefaultFeatureConfig, Guid> _defaultRepo =
        Substitute.For<IMasterRepository<DefaultFeatureConfig, Guid>>();

    private readonly IMasterUnitOfWork _masterUow = Substitute.For<IMasterUnitOfWork>();
    private readonly FeatureService _sut;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly IMasterRepository<Tenant, Guid> _tenantRepo = Substitute.For<IMasterRepository<Tenant, Guid>>();

    public FeatureServiceCachingTests()
    {
        _masterUow.Repository<TenantFeatureConfig>().Returns(_configRepo);
        _masterUow.Repository<DefaultFeatureConfig>().Returns(_defaultRepo);
        _masterUow.Repository<Tenant>().Returns(_tenantRepo);

        _configRepo.GetListAsync(Arg.Any<Expression<Func<TenantFeatureConfig, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(_ => new List<TenantFeatureConfig>());
        _defaultRepo.GetListAsync(Arg.Any<ISpecification<DefaultFeatureConfig>?>(), Arg.Any<CancellationToken>())
            .Returns(_ => new List<DefaultFeatureConfig>());

        // Non-subscription tenant: plan gating is skipped, so all features are allowed.
        _tenantRepo.GetByIdAsync(_tenantId, Arg.Any<CancellationToken>())
            .Returns(new Tenant
            {
                Id = _tenantId,
                Name = "Test",
                ConnectionString = "test",
                BillingEmail = "test@test.com",
                CompanyAddress = new()
                {
                    Line1 = "123 Test St",
                    City = "Test",
                    State = "TX",
                    ZipCode = "12345",
                    Country = "US"
                },
                IsSubscriptionRequired = false
            });

        _sut = new FeatureService(_masterUow);
    }

    private int ConfigQueryCount => _configRepo.ReceivedCalls().Count(c => c.GetMethodInfo().Name == "GetListAsync");
    private int DefaultQueryCount => _defaultRepo.ReceivedCalls().Count(c => c.GetMethodInfo().Name == "GetListAsync");
    private int TenantQueryCount => _tenantRepo.ReceivedCalls().Count(c => c.GetMethodInfo().Name == "GetByIdAsync");

    [Fact]
    public async Task IsFeatureEnabled_SameFeatureTwice_QueriesMasterOnce()
    {
        await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.AgenticDispatch);
        await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.AgenticDispatch);

        Assert.Equal(1, ConfigQueryCount);
        Assert.Equal(1, DefaultQueryCount);
        Assert.Equal(1, TenantQueryCount);
    }

    [Fact]
    public async Task IsFeatureEnabled_DifferentFeatures_StillQueriesMasterOnce()
    {
        foreach (var feature in Enum.GetValues<TenantFeature>())
        {
            await _sut.IsFeatureEnabledAsync(_tenantId, feature);
        }

        Assert.Equal(1, ConfigQueryCount);
        Assert.Equal(1, DefaultQueryCount);
        Assert.Equal(1, TenantQueryCount);
    }

    [Fact]
    public async Task IsFeatureEnabled_DifferentTenants_QueriesPerTenantButSharesDefaults()
    {
        var otherTenantId = Guid.NewGuid();

        await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.AgenticDispatch);
        await _sut.IsFeatureEnabledAsync(otherTenantId, TenantFeature.AgenticDispatch);

        Assert.Equal(2, ConfigQueryCount);
        Assert.Equal(2, TenantQueryCount);
        Assert.Equal(1, DefaultQueryCount); // defaults are tenant-independent
    }

    [Fact]
    public async Task ApplyPresetFeatures_StagesRowsIntoTheCache_SoReadsSeeThemWithoutRequerying()
    {
        Assert.True(await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.VehicleTransport));

        await _sut.ApplyPresetFeaturesAsync(_tenantId, [TenantPreset.GeneralFreight]);

        Assert.False(await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.VehicleTransport));
        Assert.Equal(1, ConfigQueryCount);
    }

    [Fact]
    public async Task GetEnabledFeatures_AfterIsFeatureEnabled_ReusesTheSameCache()
    {
        await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.AgenticDispatch);
        await _sut.GetEnabledFeaturesAsync(_tenantId);
        await _sut.GetAllFeatureStatusAsync(_tenantId);

        Assert.Equal(1, ConfigQueryCount);
        Assert.Equal(1, DefaultQueryCount);
        Assert.Equal(1, TenantQueryCount);
    }
}

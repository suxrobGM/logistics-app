using System.Linq.Expressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Infrastructure.Persistence.Services.Feature;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.Persistence.Tests.Services;

/// <summary>
/// Pins the resolution chain: admin lock, then plan gate, then the tenant's config row, and for a
/// feature with no row (added after the presets were applied) the tenant's presets. A tenant that
/// does not require a subscription skips only the plan gate.
/// </summary>
public class FeatureServiceResolutionTests
{
    private readonly IMasterRepository<TenantFeatureConfig, Guid> _configRepo =
        Substitute.For<IMasterRepository<TenantFeatureConfig, Guid>>();

    private readonly IMasterRepository<PlanFeature, Guid> _planFeatureRepo =
        Substitute.For<IMasterRepository<PlanFeature, Guid>>();

    private readonly IMasterRepository<Tenant, Guid> _tenantRepo = Substitute.For<IMasterRepository<Tenant, Guid>>();
    private readonly FeatureService _sut;
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _planId = Guid.NewGuid();

    public FeatureServiceResolutionTests()
    {
        var masterUow = Substitute.For<IMasterUnitOfWork>();
        masterUow.Repository<TenantFeatureConfig>().Returns(_configRepo);
        masterUow.Repository<PlanFeature>().Returns(_planFeatureRepo);
        masterUow.Repository<Tenant>().Returns(_tenantRepo);

        SetTenantConfigs();

        _sut = new FeatureService(masterUow);
    }

    private void SetTenant(bool isSubscriptionRequired, bool hasSubscription = false)
    {
        var tenant = new Tenant
        {
            Id = _tenantId,
            Name = "test",
            DatabaseName = "test",
            BillingEmail = "test@test.com",
            CompanyAddress = new()
            {
                Line1 = "123 Test St",
                City = "Test",
                State = "TX",
                ZipCode = "12345",
                Country = "US"
            },
            IsSubscriptionRequired = isSubscriptionRequired,
            Presets = [TenantPreset.GeneralFreight]
        };

        if (hasSubscription)
        {
            tenant.Subscription = new Subscription
            {
                TenantId = _tenantId,
                Tenant = tenant,
                PlanId = _planId,
                Plan = null!
            };
        }

        _tenantRepo.GetByIdAsync(_tenantId, Arg.Any<CancellationToken>()).Returns(tenant);
    }

    private void SetTenantConfigs(params TenantFeatureConfig[] configs)
    {
        _configRepo.GetListAsync(Arg.Any<Expression<Func<TenantFeatureConfig, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(_ => configs.ToList());
    }

    private void SetPlanFeatures(params TenantFeature[] features)
    {
        _planFeatureRepo.GetListAsync(Arg.Any<Expression<Func<PlanFeature, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(_ => features.Select(f => new PlanFeature { PlanId = _planId, Feature = f }).ToList());
    }

    [Fact]
    public async Task IsFeatureEnabled_NoConfigRow_FollowsTenantPresets()
    {
        SetTenant(isSubscriptionRequired: false);

        Assert.True(await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.AICopilot));
        Assert.False(await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.VehicleTransport));
    }

    [Fact]
    public async Task IsFeatureEnabled_NoSubscriptionRequired_ExplicitTenantDisableStillWins()
    {
        SetTenant(isSubscriptionRequired: false);
        SetTenantConfigs(new TenantFeatureConfig
        {
            TenantId = _tenantId,
            Feature = TenantFeature.AICopilot,
            IsEnabled = false
        });

        Assert.False(await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.AICopilot));
    }

    [Fact]
    public async Task IsFeatureEnabled_SubscribedTenant_PlanWithoutFeatureIsDisabled()
    {
        SetTenant(isSubscriptionRequired: true, hasSubscription: true);
        SetPlanFeatures(TenantFeature.Dashboard);

        Assert.False(await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.AICopilot));
    }

    [Fact]
    public async Task IsFeatureEnabled_SubscribedTenant_PlanWithFeatureAndNoConfigRow_FollowsTenantPresets()
    {
        SetTenant(isSubscriptionRequired: true, hasSubscription: true);
        SetPlanFeatures(TenantFeature.AICopilot, TenantFeature.VehicleTransport);

        Assert.True(await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.AICopilot));
        Assert.False(await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.VehicleTransport));
    }
}

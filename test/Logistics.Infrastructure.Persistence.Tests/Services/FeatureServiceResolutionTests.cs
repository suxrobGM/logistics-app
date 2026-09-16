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
/// Pins the resolution chain around default-off features (plan differentiators like AICopilot):
/// a tenant that does not require a subscription gets every feature, like the top plan, while
/// subscription tenants still go through plan gating and platform defaults.
/// </summary>
public class FeatureServiceResolutionTests
{
    private readonly IMasterRepository<TenantFeatureConfig, Guid> _configRepo =
        Substitute.For<IMasterRepository<TenantFeatureConfig, Guid>>();

    private readonly IMasterRepository<DefaultFeatureConfig, Guid> _defaultRepo =
        Substitute.For<IMasterRepository<DefaultFeatureConfig, Guid>>();

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
        masterUow.Repository<DefaultFeatureConfig>().Returns(_defaultRepo);
        masterUow.Repository<PlanFeature>().Returns(_planFeatureRepo);
        masterUow.Repository<Tenant>().Returns(_tenantRepo);

        SetTenantConfigs();
        _defaultRepo.GetListAsync(Arg.Any<ISpecification<DefaultFeatureConfig>?>(), Arg.Any<CancellationToken>())
            .Returns(_ => new List<DefaultFeatureConfig>
            {
                new() { Feature = TenantFeature.AICopilot, IsEnabledByDefault = false }
            });

        _sut = new FeatureService(masterUow);
    }

    private void SetTenant(bool isSubscriptionRequired, bool hasSubscription = false)
    {
        var tenant = new Tenant
        {
            Id = _tenantId,
            Name = "test",
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
            IsSubscriptionRequired = isSubscriptionRequired
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
    public async Task IsFeatureEnabled_NoSubscriptionRequired_DefaultOffFeatureIsEnabled()
    {
        SetTenant(isSubscriptionRequired: false);

        Assert.True(await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.AICopilot));
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
    public async Task IsFeatureEnabled_SubscribedTenant_PlanWithFeatureStillFollowsDefaultOff()
    {
        SetTenant(isSubscriptionRequired: true, hasSubscription: true);
        SetPlanFeatures(TenantFeature.AICopilot);

        Assert.False(await _sut.IsFeatureEnabledAsync(_tenantId, TenantFeature.AICopilot));
    }
}

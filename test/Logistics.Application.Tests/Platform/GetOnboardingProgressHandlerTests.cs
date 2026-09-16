using Logistics.Application.Abstractions.Features;
using Logistics.Application.Modules.Platform.Onboarding.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Platform;

public class GetOnboardingProgressHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IFeatureService _featureService = Substitute.For<IFeatureService>();

    private readonly ITenantRepository<Truck, Guid> _truckRepo =
        Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<Employee, Guid> _employeeRepo =
        Substitute.For<ITenantRepository<Employee, Guid>>();
    private readonly ITenantRepository<Customer, Guid> _customerRepo =
        Substitute.For<ITenantRepository<Customer, Guid>>();
    private readonly ITenantRepository<Load, Guid> _loadRepo =
        Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly ITenantRepository<EldProviderConfiguration, Guid> _eldConfigRepo =
        Substitute.For<ITenantRepository<EldProviderConfiguration, Guid>>();

    private readonly Tenant _tenant;
    private readonly GetOnboardingProgressHandler _sut;

    public GetOnboardingProgressHandlerTests()
    {
        _tenant = new Tenant
        {
            Name = "acme",
            CompanyAddress = new Address
            {
                Line1 = "",
                City = "",
                State = "",
                ZipCode = "",
                Country = ""
            },
            ConnectionString = "Host=localhost",
            BillingEmail = "billing@acme.test"
        };

        _tenantUow.GetCurrentTenant().Returns(_tenant);
        _tenantUow.Repository<Truck>().Returns(_truckRepo);
        _tenantUow.Repository<Employee>().Returns(_employeeRepo);
        _tenantUow.Repository<Customer>().Returns(_customerRepo);
        _tenantUow.Repository<Load>().Returns(_loadRepo);
        _tenantUow.Repository<EldProviderConfiguration>().Returns(_eldConfigRepo);

        _truckRepo.Query().Returns(QueryOf<Truck>(0));
        _employeeRepo.Query().Returns(QueryOf<Employee>(0));
        _customerRepo.Query().Returns(QueryOf<Customer>(0));
        _loadRepo.Query().Returns(QueryOf<Load>(0));
        _eldConfigRepo.Query().Returns(QueryOf<EldProviderConfiguration>(0));

        EnableFeatures(Enum.GetValues<TenantFeature>());

        _sut = new GetOnboardingProgressHandler(_tenantUow, _featureService);
    }

    private void EnableFeatures(params TenantFeature[] features)
    {
        _featureService.GetEnabledFeaturesAsync(Arg.Any<Guid>()).Returns(features);
    }

    private static IQueryable<T> QueryOf<T>(int count) where T : class
    {
        return Enumerable.Range(0, count).Select(_ => Substitute.For<T>()).ToList().BuildMock();
    }

    [Fact]
    public async Task Handle_FleetMode_EmitsAllSevenStepsIncomplete()
    {
        var result = await _sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(
            ["companyProfile", "addTruck", "inviteTeam", "addCustomer", "firstLoad", "getPaid", "connectEld"],
            result.Value!.Steps.Select(s => s.Key));
        Assert.All(result.Value.Steps, s => Assert.False(s.IsComplete));
    }

    [Fact]
    public async Task Handle_SoloOperator_OmitsInviteTeamStep()
    {
        _tenant.Presets = [TenantPreset.GeneralFreight, TenantPreset.SoloOperator];

        var result = await _sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.DoesNotContain(result.Value!.Steps, s => s.Key == "inviteTeam");
        Assert.Equal(6, result.Value.Steps.Count);
        _employeeRepo.DidNotReceive().Query();
    }

    [Fact]
    public async Task Handle_PopulatedTenant_MarksStepsComplete()
    {
        _tenant.CompanyAddress = new Address
        {
            Line1 = "1 Main St",
            City = "Dallas",
            State = "TX",
            ZipCode = "75001",
            Country = "US"
        };
        _tenant.ConnectStatus = StripeConnectStatus.Active;
        _truckRepo.Query().Returns(QueryOf<Truck>(3));
        _employeeRepo.Query().Returns(QueryOf<Employee>(4));
        _customerRepo.Query().Returns(QueryOf<Customer>(2));
        _loadRepo.Query().Returns(QueryOf<Load>(7));
        _eldConfigRepo.Query().Returns(QueryOf<EldProviderConfiguration>(1));

        var result = await _sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.All(result.Value!.Steps, s => Assert.True(s.IsComplete));
    }

    [Fact]
    public async Task Handle_SingleEmployee_InviteTeamIncomplete()
    {
        _employeeRepo.Query().Returns(QueryOf<Employee>(1));

        var result = await _sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.False(result.Value!.Steps.Single(s => s.Key == "inviteTeam").IsComplete);
    }

    [Theory]
    [InlineData(StripeConnectStatus.NotConnected)]
    [InlineData(StripeConnectStatus.Pending)]
    [InlineData(StripeConnectStatus.Restricted)]
    [InlineData(StripeConnectStatus.Disabled)]
    public async Task Handle_ConnectStatusNotActive_GetPaidIncomplete(StripeConnectStatus status)
    {
        _tenant.ConnectStatus = status;

        var result = await _sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.False(result.Value!.Steps.Single(s => s.Key == "getPaid").IsComplete);
    }

    [Fact]
    public async Task Handle_FeatureDisabled_OmitsItsStep()
    {
        EnableFeatures(
            [.. Enum.GetValues<TenantFeature>().Where(f => f is not TenantFeature.Eld)]);

        var result = await _sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.DoesNotContain(result.Value!.Steps, s => s.Key == "connectEld");
        _eldConfigRepo.DidNotReceive().Query();
    }

    [Fact]
    public async Task Handle_NoFeaturesEnabled_KeepsOnlyCompanyProfile()
    {
        EnableFeatures();

        var result = await _sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.Equal(["companyProfile"], result.Value!.Steps.Select(s => s.Key));
    }

    [Fact]
    public async Task Handle_PartialCompanyAddress_CompanyProfileIncomplete()
    {
        _tenant.CompanyAddress = new Address
        {
            Line1 = "1 Main St",
            City = "Dallas",
            State = "TX",
            ZipCode = "",
            Country = "US"
        };

        var result = await _sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.False(result.Value!.Steps.Single(s => s.Key == "companyProfile").IsComplete);
    }
}

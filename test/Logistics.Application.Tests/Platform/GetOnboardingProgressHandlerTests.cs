using Logistics.Application.Modules.Platform.Onboarding.Queries;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Platform;

public class GetOnboardingProgressHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();

    private readonly ITenantRepository<Truck, Guid> truckRepo =
        Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<Employee, Guid> employeeRepo =
        Substitute.For<ITenantRepository<Employee, Guid>>();
    private readonly ITenantRepository<Customer, Guid> customerRepo =
        Substitute.For<ITenantRepository<Customer, Guid>>();
    private readonly ITenantRepository<Load, Guid> loadRepo =
        Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly ITenantRepository<EldProviderConfiguration, Guid> eldConfigRepo =
        Substitute.For<ITenantRepository<EldProviderConfiguration, Guid>>();

    private readonly Tenant tenant;
    private readonly GetOnboardingProgressHandler sut;

    public GetOnboardingProgressHandlerTests()
    {
        tenant = new Tenant
        {
            Name = "acme",
            CompanyAddress = new Address
            {
                Line1 = "", City = "", State = "", ZipCode = "", Country = ""
            },
            ConnectionString = "Host=localhost",
            BillingEmail = "billing@acme.test"
        };

        tenantUow.GetCurrentTenant().Returns(tenant);
        tenantUow.Repository<Truck>().Returns(truckRepo);
        tenantUow.Repository<Employee>().Returns(employeeRepo);
        tenantUow.Repository<Customer>().Returns(customerRepo);
        tenantUow.Repository<Load>().Returns(loadRepo);
        tenantUow.Repository<EldProviderConfiguration>().Returns(eldConfigRepo);

        truckRepo.CountAsync().ReturnsForAnyArgs(0);
        employeeRepo.CountAsync().ReturnsForAnyArgs(0);
        customerRepo.CountAsync().ReturnsForAnyArgs(0);
        loadRepo.CountAsync().ReturnsForAnyArgs(0);
        eldConfigRepo.CountAsync().ReturnsForAnyArgs(0);

        sut = new GetOnboardingProgressHandler(tenantUow);
    }

    [Fact]
    public async Task Handle_FleetMode_EmitsAllSevenStepsIncomplete()
    {
        var result = await sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(OperatingMode.Fleet, result.Value!.OperatingMode);
        Assert.Equal(
            ["companyProfile", "addTruck", "inviteTeam", "addCustomer", "firstLoad", "getPaid", "connectEld"],
            result.Value.Steps.Select(s => s.Key));
        Assert.All(result.Value.Steps, s => Assert.False(s.IsComplete));
    }

    [Fact]
    public async Task Handle_SoloOperator_OmitsInviteTeamStep()
    {
        tenant.Settings.OperatingMode = OperatingMode.SoloOperator;

        var result = await sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.Equal(OperatingMode.SoloOperator, result.Value!.OperatingMode);
        Assert.DoesNotContain(result.Value.Steps, s => s.Key == "inviteTeam");
        Assert.Equal(6, result.Value.Steps.Count);
        await employeeRepo.DidNotReceiveWithAnyArgs().CountAsync();
    }

    [Fact]
    public async Task Handle_PopulatedTenant_MarksStepsComplete()
    {
        tenant.CompanyAddress = new Address
        {
            Line1 = "1 Main St", City = "Dallas", State = "TX", ZipCode = "75001", Country = "US"
        };
        tenant.ConnectStatus = StripeConnectStatus.Active;
        truckRepo.CountAsync().ReturnsForAnyArgs(3);
        employeeRepo.CountAsync().ReturnsForAnyArgs(4);
        customerRepo.CountAsync().ReturnsForAnyArgs(2);
        loadRepo.CountAsync().ReturnsForAnyArgs(7);
        eldConfigRepo.CountAsync().ReturnsForAnyArgs(1);

        var result = await sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.All(result.Value!.Steps, s => Assert.True(s.IsComplete));
    }

    [Fact]
    public async Task Handle_SingleEmployee_InviteTeamIncomplete()
    {
        employeeRepo.CountAsync().ReturnsForAnyArgs(1);

        var result = await sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.False(result.Value!.Steps.Single(s => s.Key == "inviteTeam").IsComplete);
    }

    [Theory]
    [InlineData(StripeConnectStatus.NotConnected)]
    [InlineData(StripeConnectStatus.Pending)]
    [InlineData(StripeConnectStatus.Restricted)]
    [InlineData(StripeConnectStatus.Disabled)]
    public async Task Handle_ConnectStatusNotActive_GetPaidIncomplete(StripeConnectStatus status)
    {
        tenant.ConnectStatus = status;

        var result = await sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.False(result.Value!.Steps.Single(s => s.Key == "getPaid").IsComplete);
    }

    [Fact]
    public async Task Handle_PartialCompanyAddress_CompanyProfileIncomplete()
    {
        tenant.CompanyAddress = new Address
        {
            Line1 = "1 Main St", City = "Dallas", State = "TX", ZipCode = "", Country = "US"
        };

        var result = await sut.Handle(new GetOnboardingProgressQuery(), CancellationToken.None);

        Assert.False(result.Value!.Steps.Single(s => s.Key == "companyProfile").IsComplete);
    }
}

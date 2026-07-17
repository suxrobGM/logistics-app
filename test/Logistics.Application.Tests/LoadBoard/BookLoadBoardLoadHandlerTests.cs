using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.LoadBoard;

public class BookLoadBoardLoadHandlerTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ILoadBoardProviderFactory providerFactory = Substitute.For<ILoadBoardProviderFactory>();
    private readonly ILoadBoardProviderService provider = Substitute.For<ILoadBoardProviderService>();
    private readonly IBrokerCreditService brokerCreditService = Substitute.For<IBrokerCreditService>();

    private readonly ITenantRepository<LoadBoardListing, Guid> listingRepo =
        Substitute.For<ITenantRepository<LoadBoardListing, Guid>>();
    private readonly ITenantRepository<LoadBoardConfiguration, Guid> configRepo =
        Substitute.For<ITenantRepository<LoadBoardConfiguration, Guid>>();
    private readonly ITenantRepository<Truck, Guid> truckRepo =
        Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<Employee, Guid> employeeRepo =
        Substitute.For<ITenantRepository<Employee, Guid>>();
    private readonly ITenantRepository<Customer, Guid> customerRepo =
        Substitute.For<ITenantRepository<Customer, Guid>>();
    private readonly ITenantRepository<Load, Guid> loadRepo =
        Substitute.For<ITenantRepository<Load, Guid>>();

    private readonly Tenant tenant;
    private readonly LoadBoardListing listing;
    private readonly BookLoadBoardLoadCommand command;
    private readonly BookLoadBoardLoadHandler sut;

    public BookLoadBoardLoadHandlerTests()
    {
        tenant = new Tenant
        {
            Name = "test",
            ConnectionString = "test",
            BillingEmail = "billing@test.com",
            CompanyAddress = new Address { Line1 = "1 Test St", City = "Test", State = "TX", ZipCode = "00000", Country = "US" }
        };
        listing = CreateListing();

        var truck = new Truck { Number = "T-100", Type = TruckType.FreightTruck };
        var dispatcher = new Employee { Email = "dispatcher@test.com", FirstName = "Dana", LastName = "Doe" };
        var config = new LoadBoardConfiguration { ProviderType = LoadBoardProviderType.Demo, ApiKey = "demo" };

        command = new BookLoadBoardLoadCommand
        {
            ListingId = listing.Id,
            TruckId = truck.Id,
            DispatcherId = dispatcher.Id
        };

        tenantUow.Repository<LoadBoardListing>().Returns(listingRepo);
        tenantUow.Repository<LoadBoardConfiguration>().Returns(configRepo);
        tenantUow.Repository<Truck>().Returns(truckRepo);
        tenantUow.Repository<Employee>().Returns(employeeRepo);
        tenantUow.Repository<Customer>().Returns(customerRepo);
        tenantUow.Repository<Load>().Returns(loadRepo);
        tenantUow.GetCurrentTenant().Returns(tenant);

        listingRepo.GetByIdAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
        configRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardConfiguration, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(config);
        truckRepo.GetByIdAsync(truck.Id, Arg.Any<CancellationToken>()).Returns(truck);
        employeeRepo.GetByIdAsync(dispatcher.Id, Arg.Any<CancellationToken>()).Returns(dispatcher);
        customerRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Customer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        providerFactory.GetProvider(config).Returns(provider);
        provider.BookLoadAsync(Arg.Any<string>(), Arg.Any<LoadBoardBookingRequest>())
            .Returns(new LoadBoardBookingResultDto { Success = true, ExternalConfirmationId = "CONF-1" });

        sut = new BookLoadBoardLoadHandler(
            tenantUow, providerFactory, brokerCreditService, NullLogger<BookLoadBoardLoadHandler>.Instance);
    }

    private static LoadBoardListing CreateListing()
    {
        var address = new Address { Line1 = "1 St", City = "Dallas", State = "TX", ZipCode = "75001", Country = "US" };
        return new LoadBoardListing
        {
            ExternalListingId = "EXT-1",
            ProviderType = LoadBoardProviderType.Demo,
            OriginAddress = address,
            OriginLocation = new GeoPoint(-96.8, 32.8),
            DestinationAddress = address,
            DestinationLocation = new GeoPoint(-87.6, 41.9),
            BrokerName = "Test Broker",
            BrokerMcNumber = "MC123456",
            ExpiresAt = DateTime.UtcNow.AddDays(1)
        };
    }

    private void SetupCredit(int? score, bool? authorityActive = true)
    {
        brokerCreditService.GetBrokerCreditAsync(listing.BrokerMcNumber, Arg.Any<CancellationToken>())
            .Returns(new BrokerCreditDto
            {
                McNumber = "123456",
                CreditScore = score,
                DaysToPay = 30,
                AuthorityActive = authorityActive,
                Source = BrokerCreditSource.Demo,
                CheckedAt = DateTime.UtcNow
            });
    }

    #region Credit gate

    [Fact]
    public async Task Handle_NoThresholdConfigured_Books()
    {
        tenant.Settings.MinBrokerCreditScore = null;
        SetupCredit(score: 10);

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await provider.Received(1).BookLoadAsync(Arg.Any<string>(), Arg.Any<LoadBoardBookingRequest>());
    }

    [Fact]
    public async Task Handle_ScoreBelowThreshold_BlocksWithErrorCode()
    {
        tenant.Settings.MinBrokerCreditScore = 70;
        SetupCredit(score: 50);

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BrokerCreditBelowThreshold, result.ErrorCode);
        await provider.DidNotReceiveWithAnyArgs().BookLoadAsync(default!, default!);
    }

    [Fact]
    public async Task Handle_ScoreBelowThreshold_OverrideBooks()
    {
        tenant.Settings.MinBrokerCreditScore = 70;
        SetupCredit(score: 50);
        command.OverrideCreditCheck = true;

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await provider.Received(1).BookLoadAsync(Arg.Any<string>(), Arg.Any<LoadBoardBookingRequest>());
    }

    [Fact]
    public async Task Handle_MissingScore_NeverBlocks()
    {
        tenant.Settings.MinBrokerCreditScore = 70;
        brokerCreditService.GetBrokerCreditAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((BrokerCreditDto?)null);

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await provider.Received(1).BookLoadAsync(Arg.Any<string>(), Arg.Any<LoadBoardBookingRequest>());
    }

    [Fact]
    public async Task Handle_InactiveAuthority_BlocksEvenWithoutThreshold()
    {
        tenant.Settings.MinBrokerCreditScore = null;
        SetupCredit(score: 90, authorityActive: false);

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BrokerCreditBelowThreshold, result.ErrorCode);
        Assert.Contains("authority", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_BlockedBooking_StillStampsCreditOnListing()
    {
        tenant.Settings.MinBrokerCreditScore = 70;
        SetupCredit(score: 50);

        await sut.Handle(command, CancellationToken.None);

        Assert.Equal(50, listing.BrokerCreditScore);
        Assert.Equal(30, listing.BrokerDaysToPay);
        Assert.NotNull(listing.BrokerCreditCheckedAt);
        await tenantUow.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ScoreAtThreshold_Books()
    {
        tenant.Settings.MinBrokerCreditScore = 70;
        SetupCredit(score: 70);

        var result = await sut.Handle(command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    #endregion
}

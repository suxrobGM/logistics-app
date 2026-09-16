using Logistics.Application.Abstractions.AIDispatch;
using Logistics.Application.Abstractions.LoadBoard;
using Logistics.Application.Modules.Integrations.LoadBoard.Commands;
using Logistics.Application.Modules.Integrations.LoadBoard.Services;
using Logistics.Application.Modules.Integrations.Negotiation.Services;
using Logistics.Application.Modules.Operations.Common.Services;
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
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ILoadBoardTokenService _tokenService = Substitute.For<ILoadBoardTokenService>();
    private readonly ILoadBoardProviderService _provider = Substitute.For<ILoadBoardProviderService>();
    private readonly IBrokerCreditService _brokerCreditService = Substitute.For<IBrokerCreditService>();
    private readonly IInboundEmailRouteRegistry _routeRegistry = Substitute.For<IInboundEmailRouteRegistry>();
    private readonly IAIDispatchBroadcastService _broadcastService = Substitute.For<IAIDispatchBroadcastService>();

    private readonly ITenantRepository<LoadBoardListing, Guid> _listingRepo =
        Substitute.For<ITenantRepository<LoadBoardListing, Guid>>();
    private readonly ITenantRepository<LoadBoardConfiguration, Guid> _configRepo =
        Substitute.For<ITenantRepository<LoadBoardConfiguration, Guid>>();
    private readonly ITenantRepository<Truck, Guid> _truckRepo =
        Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<Employee, Guid> _employeeRepo =
        Substitute.For<ITenantRepository<Employee, Guid>>();
    private readonly ITenantRepository<Customer, Guid> _customerRepo =
        Substitute.For<ITenantRepository<Customer, Guid>>();
    private readonly ITenantRepository<Load, Guid> _loadRepo =
        Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly ITenantRepository<RateNegotiation, Guid> _negotiationRepo =
        Substitute.For<ITenantRepository<RateNegotiation, Guid>>();

    private readonly Tenant _tenant;
    private readonly LoadBoardListing _listing;
    private readonly BookLoadBoardLoadCommand _command;
    private readonly BookLoadBoardLoadHandler _sut;

    public BookLoadBoardLoadHandlerTests()
    {
        _tenant = new Tenant
        {
            Name = "test",
            ConnectionString = "test",
            BillingEmail = "billing@test.com",
            CompanyAddress = new Address { Line1 = "1 Test St", City = "Test", State = "TX", ZipCode = "00000", Country = "US" }
        };
        _listing = CreateListing();

        var truck = new Truck { Number = "T-100", Type = TruckType.FreightTruck };
        var dispatcher = new Employee { Email = "dispatcher@test.com", FirstName = "Dana", LastName = "Doe" };
        var config = new LoadBoardConfiguration { ProviderType = LoadBoardProviderType.Demo, ApiKey = "demo" };

        _command = new BookLoadBoardLoadCommand
        {
            ListingId = _listing.Id,
            TruckId = truck.Id,
            DispatcherId = dispatcher.Id
        };

        _tenantUow.Repository<LoadBoardListing>().Returns(_listingRepo);
        _tenantUow.Repository<LoadBoardConfiguration>().Returns(_configRepo);
        _tenantUow.Repository<Truck>().Returns(_truckRepo);
        _tenantUow.Repository<Employee>().Returns(_employeeRepo);
        _tenantUow.Repository<Customer>().Returns(_customerRepo);
        _tenantUow.Repository<Load>().Returns(_loadRepo);
        _tenantUow.Repository<RateNegotiation>().Returns(_negotiationRepo);
        _tenantUow.GetCurrentTenant().Returns(_tenant);

        _listingRepo.GetByIdAsync(_listing.Id, Arg.Any<CancellationToken>()).Returns(_listing);
        _configRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<LoadBoardConfiguration, bool>>>(), Arg.Any<CancellationToken>())
            .Returns(config);
        _truckRepo.GetByIdAsync(truck.Id, Arg.Any<CancellationToken>()).Returns(truck);
        _employeeRepo.GetByIdAsync(dispatcher.Id, Arg.Any<CancellationToken>()).Returns(dispatcher);
        _customerRepo.GetAsync(Arg.Any<System.Linq.Expressions.Expression<Func<Customer, bool>>>(), Arg.Any<CancellationToken>())
            .Returns((Customer?)null);

        _tokenService.GetReadyProviderAsync(config, Arg.Any<CancellationToken>())
            .Returns(Result<ILoadBoardProviderService>.Ok(_provider));
        _provider.BookLoadAsync(Arg.Any<string>(), Arg.Any<LoadBoardBookingRequest>())
            .Returns(new LoadBoardBookingResultDto { Success = true, ExternalConfirmationId = "CONF-1" });

        var vehicleTransportGuard = Substitute.For<IVehicleTransportGuard>();
        vehicleTransportGuard.CheckLoadTypeAsync(Arg.Any<LoadType?>()).Returns(Result.Ok());

        _sut = new BookLoadBoardLoadHandler(
            _tenantUow, vehicleTransportGuard, _tokenService, _brokerCreditService, _routeRegistry, _broadcastService,
            NullLogger<BookLoadBoardLoadHandler>.Instance);
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
        _brokerCreditService.GetBrokerCreditAsync(_listing.BrokerMcNumber, Arg.Any<CancellationToken>())
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
        _tenant.Settings.MinBrokerCreditScore = null;
        SetupCredit(score: 10);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _provider.Received(1).BookLoadAsync(Arg.Any<string>(), Arg.Any<LoadBoardBookingRequest>());
    }

    [Fact]
    public async Task Handle_ScoreBelowThreshold_BlocksWithErrorCode()
    {
        _tenant.Settings.MinBrokerCreditScore = 70;
        SetupCredit(score: 50);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BrokerCreditBelowThreshold, result.ErrorCode);
        await _provider.DidNotReceiveWithAnyArgs().BookLoadAsync(default!, default!);
    }

    [Fact]
    public async Task Handle_ScoreBelowThreshold_OverrideBooks()
    {
        _tenant.Settings.MinBrokerCreditScore = 70;
        SetupCredit(score: 50);
        _command.OverrideCreditCheck = true;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _provider.Received(1).BookLoadAsync(Arg.Any<string>(), Arg.Any<LoadBoardBookingRequest>());
    }

    [Fact]
    public async Task Handle_MissingScore_NeverBlocks()
    {
        _tenant.Settings.MinBrokerCreditScore = 70;
        _brokerCreditService.GetBrokerCreditAsync(Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns((BrokerCreditDto?)null);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _provider.Received(1).BookLoadAsync(Arg.Any<string>(), Arg.Any<LoadBoardBookingRequest>());
    }

    [Fact]
    public async Task Handle_InactiveAuthority_BlocksEvenWithoutThreshold()
    {
        _tenant.Settings.MinBrokerCreditScore = null;
        SetupCredit(score: 90, authorityActive: false);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.BrokerCreditBelowThreshold, result.ErrorCode);
        Assert.Contains("authority", result.Error, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Handle_BlockedBooking_StillStampsCreditOnListing()
    {
        _tenant.Settings.MinBrokerCreditScore = 70;
        SetupCredit(score: 50);

        await _sut.Handle(_command, CancellationToken.None);

        Assert.Equal(50, _listing.BrokerCreditScore);
        Assert.Equal(30, _listing.BrokerDaysToPay);
        Assert.NotNull(_listing.BrokerCreditCheckedAt);
        await _tenantUow.Received().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ScoreAtThreshold_Books()
    {
        _tenant.Settings.MinBrokerCreditScore = 70;
        SetupCredit(score: 70);

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
    }

    #endregion

    #region Negotiated rate

    private RateNegotiation SetupNegotiation(decimal? floorTotal)
    {
        var negotiation = RateNegotiation.Create(_listing.Id, "broker@example.com", RateFloorSnapshot.None);
        negotiation.FloorTotalRate = floorTotal.HasValue
            ? new Money { Amount = floorTotal.Value, Currency = "USD" }
            : null;

        _negotiationRepo.GetAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<RateNegotiation, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(negotiation);

        return negotiation;
    }

    [Fact]
    public async Task Handle_NegotiatedRateWithoutThread_Fails()
    {
        _command.NegotiatedTotalRate = 2200m;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        await _provider.DidNotReceiveWithAnyArgs().BookLoadAsync(default!, default!);
    }

    [Fact]
    public async Task Handle_NegotiatedRateBelowThreadFloor_Fails()
    {
        SetupNegotiation(floorTotal: 2000m);
        _command.NegotiatedTotalRate = 1900m;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NegotiationBelowFloor, result.ErrorCode);
        await _provider.DidNotReceiveWithAnyArgs().BookLoadAsync(default!, default!);
    }

    [Fact]
    public async Task Handle_NegotiatedRateAtOrAboveFloor_BooksAndAcceptsThread()
    {
        var negotiation = SetupNegotiation(floorTotal: 2000m);
        _command.NegotiatedTotalRate = 2200m;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(RateNegotiationStatus.Accepted, negotiation.Status);
        Assert.Equal(result.Value!.CreatedLoadId, negotiation.LoadId);
        await _loadRepo.Received(1).AddAsync(
            Arg.Is<Load>(l => l.DeliveryCost.Amount == 2200m), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AcceptedThread_RevokesItsReplyRoute()
    {
        var negotiation = SetupNegotiation(floorTotal: 2000m);
        _command.NegotiatedTotalRate = 2200m;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _routeRegistry.Received(1).RevokeAsync(
            Arg.Is<IEnumerable<string>>(t => t.Single() == negotiation.ReplyToken),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_OmittedNegotiatedRateBelowThreadFloor_Fails()
    {
        SetupNegotiation(floorTotal: 2000m);
        _listing.TotalRate = new Money { Amount = 1800m, Currency = "USD" };

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NegotiationBelowFloor, result.ErrorCode);
        await _provider.DidNotReceiveWithAnyArgs().BookLoadAsync(default!, default!);
    }

    [Fact]
    public async Task Handle_OmittedNegotiatedRateAboveThreadFloor_BooksAtTheListingRate()
    {
        SetupNegotiation(floorTotal: 2000m);
        _listing.TotalRate = new Money { Amount = 2500m, Currency = "USD" };

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.True(result.IsSuccess);
        await _loadRepo.Received(1).AddAsync(
            Arg.Is<Load>(l => l.DeliveryCost.Amount == 2500m), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_AcceptedThread_BroadcastsTheClosedThread()
    {
        var negotiation = SetupNegotiation(floorTotal: 2000m);
        _command.NegotiatedTotalRate = 2200m;

        await _sut.Handle(_command, CancellationToken.None);

        await _broadcastService.Received(1).BroadcastNegotiationAsync(
            _tenant.Id,
            Arg.Is<RateNegotiationDto>(d =>
                d.Id == negotiation.Id && d.Status == RateNegotiationStatus.Accepted));
    }

    [Fact]
    public async Task Handle_ThreadWithPerMileOnlyFloor_RefusesUncheckedRate()
    {
        SetupNegotiation(floorTotal: null);
        _command.NegotiatedTotalRate = 2200m;

        var result = await _sut.Handle(_command, CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorCodes.NegotiationFloorMissing, result.ErrorCode);
        await _provider.DidNotReceiveWithAnyArgs().BookLoadAsync(default!, default!);
    }

    #endregion
}

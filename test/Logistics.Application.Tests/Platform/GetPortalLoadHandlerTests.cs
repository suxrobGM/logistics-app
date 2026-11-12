using Logistics.Application.Modules.Operations.Tracking.Services;
using Logistics.Application.Modules.Platform.Portal.Queries;
using Logistics.Application.Tests.Operations.Loads;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Platform;

public class GetPortalLoadHandlerTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITrackingLinkService _trackingLinks = Substitute.For<ITrackingLinkService>();
    private readonly Load _load = LoadInvoiceChargesTests.NewLoad();
    private readonly GetPortalLoadHandler _sut;

    public GetPortalLoadHandlerTests()
    {
        var loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
        loadRepo.Query().Returns(_ => new List<Load> { _load }.BuildMock());
        _tenantUow.Repository<Load>().Returns(loadRepo);
        _sut = new GetPortalLoadHandler(_tenantUow, _trackingLinks);
    }

    [Fact]
    public async Task Handle_OtherCustomersLoad_IsNotFound()
    {
        var result = await _sut.Handle(
            new GetPortalLoadQuery { LoadId = _load.Id, CustomerId = Guid.NewGuid() }, default);

        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_OwnLoad_ReturnsScheduleEtaAndOnlySentUpdates()
    {
        var truck = new Truck { Number = "T-1", Type = TruckType.FreightTruck };
        var trip = Trip.Create("Trip", truck, [_load]);
        var dropOff = trip.Stops.Single(s => s.Type == TripStopType.DropOff);
        foreach (var stop in trip.Stops.Where(s => !_load.TripStops.Contains(s)))
        {
            _load.TripStops.Add(stop);
        }

        trip.Stops.Single(s => s.Type == TripStopType.PickUp).ArrivedAt = DateTime.UtcNow.AddHours(-2);
        dropOff.EstimatedArrivalAt = DateTime.UtcNow.AddHours(3);
        dropOff.LateNotifiedAt = DateTime.UtcNow;
        _load.DeliveryAppointmentTo = DateTime.UtcNow.AddHours(1);
        _load.RecordCustomerUpdate(CustomerUpdateEvent.Dispatched, CustomerUpdateOutcome.Sent, 1);
        _load.RecordCustomerUpdate(CustomerUpdateEvent.PickedUp, CustomerUpdateOutcome.Skipped, 0, "Turned off");
        var link = new TrackingLink { Token = "t", LoadId = _load.Id, ExpiresAt = DateTime.UtcNow.AddDays(10) };
        _trackingLinks.FindActiveAsync(_load.Id, Arg.Any<CancellationToken>()).Returns(link);
        _trackingLinks.GetUrl(link).Returns("https://portal.test/track/x/t");

        var result = await _sut.Handle(
            new GetPortalLoadQuery { LoadId = _load.Id, CustomerId = _load.CustomerId }, default);

        var dto = result.Value!;
        Assert.Equal(_load.DeliveryAppointmentTo, dto.DeliveryAppointmentTo);
        Assert.Equal(dropOff.EstimatedArrivalAt, dto.EstimatedArrivalAt);
        Assert.True(dto.IsRunningLate);
        Assert.Equal("https://portal.test/track/x/t", dto.TrackingUrl);
        Assert.Equal(CustomerUpdateEvent.Dispatched, Assert.Single(dto.Updates).Event);
    }
}

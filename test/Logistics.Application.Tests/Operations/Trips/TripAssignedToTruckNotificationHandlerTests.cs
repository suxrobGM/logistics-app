using Logistics.Application.Abstractions.Notifications;
using Logistics.Application.Modules.Operations.Trips.Events;
using Logistics.Application.Tests.Operations.Loads;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class TripAssignedToTruckNotificationHandlerTests
{
    private readonly IPushNotificationService _push = Substitute.For<IPushNotificationService>();
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Trip, Guid> _tripRepo = Substitute.For<ITenantRepository<Trip, Guid>>();
    private readonly TripAssignedToTruckNotificationHandler _sut;

    public TripAssignedToTruckNotificationHandlerTests()
    {
        _tenantUow.Repository<Trip>().Returns(_tripRepo);
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());
        _sut = new TripAssignedToTruckNotificationHandler(
            _push,
            Substitute.For<INotificationService>(),
            Substitute.For<ITelegramNotificationService>(),
            _tenantUow,
            NullLogger<TripAssignedToTruckNotificationHandler>.Instance);
    }

    /// <summary>The event is raised before the insert assigns the number, so it must come off the saved trip.</summary>
    [Fact]
    public async Task Handle_UsesNumberFromTrackedTrip()
    {
        var trip = Trip.Create("Trip", null, [LoadInvoiceChargesTests.NewLoad()]);
        typeof(Trip).GetProperty(nameof(Trip.Number))!.SetValue(trip, 5678L);
        _tripRepo.GetByIdAsync(trip.Id, Arg.Any<CancellationToken>()).Returns(trip);

        await _sut.Handle(
            new TripAssignedToTruckEvent(trip.Id, "Trip", Guid.NewGuid(), "T-1", "token", "Driver", null, null),
            CancellationToken.None);

        await _push.Received(1).SendNotificationAsync(
            Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("#5678")),
            "token",
            Arg.Any<IReadOnlyDictionary<string, string>?>());
    }
}

using Logistics.Application.Modules.Operations.Notifications.Services;
using Logistics.Application.Modules.Operations.Trips.Events;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Persistence;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class TripAssignedToTruckNotificationHandlerTests
{
    private readonly IEventNotifier _notifier = Substitute.For<IEventNotifier>();
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly TripAssignedToTruckNotificationHandler _sut;

    public TripAssignedToTruckNotificationHandlerTests()
    {
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());
        _sut = new TripAssignedToTruckNotificationHandler(_notifier, _tenantUow);
    }

    /// <summary>The event is raised before the insert assigns the number, so it must come off the saved trip.</summary>
    [Fact]
    public async Task Handle_UsesNumberFromTrackedTrip()
    {
        var trip = Trip.Create("Trip", null, [TestData.Load()]);
        typeof(Trip).GetProperty(nameof(Trip.Number))!.SetValue(trip, 5678L);
        _tenantUow.Stub(trip);

        await _sut.Handle(
            new TripAssignedToTruckEvent(trip.Id, "Trip", Guid.NewGuid(), "T-1", "token", "Driver", null, null),
            CancellationToken.None);

        await _notifier.Received(1).PushAsync(
            Arg.Any<string>(),
            Arg.Is<string>(body => body.Contains("#5678")),
            Arg.Any<IReadOnlyDictionary<string, string>?>(),
            Arg.Any<string?[]>());
    }
}

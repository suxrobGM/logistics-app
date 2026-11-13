using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Events;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class TripEtaTests
{
    private static readonly DateTime Now = new(2026, 11, 12, 12, 0, 0, DateTimeKind.Utc);

    private readonly Truck _truck = TestData.Truck();

    private Trip NewTrip(DateTime? deliveryAppointmentEnd)
    {
        var load = Load.Create("Load", LoadType.GeneralFreight, 1000, TestData.Address(), new GeoPoint(0, 0), TestData.Address(),
            new GeoPoint(1, 1), new Customer { Name = "Acme" }, _truck, TestData.Driver(), "USD");
        load.DeliveryAppointmentTo = deliveryAppointmentEnd;
        var trip = Trip.Create("Trip", _truck, [load]);
        trip.DomainEvents.Clear();
        return trip;
    }

    private static TripStop DropOff(Trip trip) => trip.Stops.Single(s => s.Type == TripStopType.DropOff);

    [Fact]
    public void ApplyArrivalEstimates_PastAppointmentPlusGrace_StampsAndRaisesLateOnce()
    {
        var trip = NewTrip(Now.AddHours(2));
        var stop = DropOff(trip);
        var lateEta = Now.AddHours(3);

        trip.ApplyArrivalEstimates(new Dictionary<Guid, DateTime> { [stop.Id] = lateEta }, Now);
        trip.ApplyArrivalEstimates(new Dictionary<Guid, DateTime> { [stop.Id] = lateEta }, Now.AddMinutes(10));

        var late = Assert.Single(trip.DomainEvents.OfType<TripStopRunningLateEvent>());
        Assert.Equal(stop.Id, late.StopId);
        Assert.Equal(Now, stop.LateNotifiedAt);
        Assert.Equal(lateEta, stop.EstimatedArrivalAt);
    }

    [Fact]
    public void ApplyArrivalEstimates_WithinGrace_IsNotLate()
    {
        var trip = NewTrip(Now.AddHours(2));
        var stop = DropOff(trip);

        trip.ApplyArrivalEstimates(new Dictionary<Guid, DateTime> { [stop.Id] = Now.AddHours(2).AddMinutes(20) }, Now);

        Assert.Empty(trip.DomainEvents.OfType<TripStopRunningLateEvent>());
        Assert.Null(stop.LateNotifiedAt);
    }

    [Fact]
    public void ApplyArrivalEstimates_NoAppointment_IsNeverLate()
    {
        var trip = NewTrip(null);
        var stop = DropOff(trip);

        trip.ApplyArrivalEstimates(new Dictionary<Guid, DateTime> { [stop.Id] = Now.AddDays(3) }, Now);

        Assert.Empty(trip.DomainEvents.OfType<TripStopRunningLateEvent>());
    }

    [Fact]
    public void IsEtaDue_RespectsTheRefreshInterval()
    {
        var trip = NewTrip(null);

        Assert.True(trip.IsEtaDue(Now));
        trip.ApplyArrivalEstimates(new Dictionary<Guid, DateTime>(), Now);

        Assert.False(trip.IsEtaDue(Now.AddMinutes(9)));
        Assert.True(trip.IsEtaDue(Now.Add(Trip.EtaRefreshInterval)));
    }
}

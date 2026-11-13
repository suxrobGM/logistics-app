using System.Text.Json;
using System.Text.Json.Nodes;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.AI.Tools.Dispatch;
using NSubstitute;
using Xunit;

namespace Logistics.Infrastructure.AI.Tests.Tools.Dispatch;

public class GetTripToolTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Trip, Guid> _tripRepo = Substitute.For<ITenantRepository<Trip, Guid>>();
    private readonly GetTripTool _sut;

    public GetTripToolTests()
    {
        _tenantUow.Repository<Trip>().Returns(_tripRepo);
        _sut = new GetTripTool(_tenantUow);
    }

    [Fact]
    public async Task Execute_UnknownTrip_ReturnsError()
    {
        var result = await _sut.ExecuteAsync(
            new JsonObject { ["trip_id"] = Guid.NewGuid().ToString() }, CancellationToken.None);

        Assert.Contains("Trip not found", result);
    }

    [Fact]
    public async Task Execute_KnownTrip_ReturnsStopsInOrderWithEtaAndLateFlag()
    {
        var address = new Address { Line1 = "1 Depot", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" };
        var load = new Load
        {
            Name = "Load",
            Type = LoadType.GeneralFreight,
            Customer = null!,
            OriginAddress = address,
            OriginLocation = new GeoPoint(-96.8, 32.78),
            DestinationAddress = address,
            DestinationLocation = new GeoPoint(-95.36, 29.76),
            DeliveryCost = 1000m,
            DeliveryAppointmentTo = DateTime.UtcNow.AddHours(1)
        };
        var trip = Trip.Create("Trip", new Truck { Number = "T-1", Type = TruckType.FreightTruck }, [load]);
        var dropOff = trip.Stops.Single(s => s.Type == TripStopType.DropOff);
        dropOff.EstimatedArrivalAt = DateTime.UtcNow.AddHours(3);
        dropOff.LateNotifiedAt = DateTime.UtcNow;
        _tripRepo.GetByIdAsync(trip.Id, Arg.Any<CancellationToken>()).Returns(trip);

        var result = await _sut.ExecuteAsync(
            new JsonObject { ["trip_id"] = trip.Id.ToString() }, CancellationToken.None);

        var stops = JsonDocument.Parse(result).RootElement.GetProperty("stops").EnumerateArray().ToList();
        Assert.Equal(["PickUp", "DropOff"], stops.Select(s => s.GetProperty("type").GetString()));
        Assert.True(stops[1].GetProperty("running_late").GetBoolean());
        Assert.NotNull(stops[1].GetProperty("estimated_arrival_at").GetString());
        Assert.False(stops[0].GetProperty("running_late").GetBoolean());
    }
}

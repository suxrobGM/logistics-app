using Logistics.Application.Abstractions.Notifications;
using Logistics.Application.Modules.Operations.Trips.Services;
using Logistics.Application.Tests.Operations.Loads;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Operations.Trips;

public class DetentionAlertServiceTests
{
    private static readonly DateTime Now = new(2026, 10, 12, 15, 0, 0, DateTimeKind.Utc);

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly INotificationService _notifications = Substitute.For<INotificationService>();
    private readonly Trip _trip;
    private readonly DetentionAlertService _sut;

    public DetentionAlertServiceTests()
    {
        var truck = new Truck { Number = "T-7", Type = TruckType.FreightTruck };
        _trip = Trip.Create("Trip", truck, [LoadInvoiceChargesTests.NewLoad()]);

        var stopRepo = Substitute.For<ITenantRepository<TripStop, Guid>>();
        stopRepo.Query().Returns(_ => _trip.Stops.BuildMock());
        _tenantUow.Repository<TripStop>().Returns(stopRepo);
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());

        _sut = new DetentionAlertService(_tenantUow, _notifications);
    }

    [Fact]
    public async Task SendAlerts_OverFreeTime_AlertsOnceAndStampsTheStop()
    {
        var stop = _trip.Stops[0];
        stop.ArrivedAt = Now.AddMinutes(-150);

        var first = await _sut.SendAlertsAsync(Now);
        var second = await _sut.SendAlertsAsync(Now.AddMinutes(15));

        Assert.Equal(1, first);
        Assert.Equal(0, second);
        Assert.Equal(Now, stop.DetentionAlertedAt);
        await _notifications.Received(1).SendNotificationAsync(
            Arg.Is<string>(t => t.Contains("pickup")), Arg.Is<string>(m => m.Contains("Truck T-7") && m.Contains("2h 30m")));
    }

    [Fact]
    public async Task SendAlerts_WithinFreeTimeOrDeparted_SendsNothing()
    {
        _trip.Stops[0].ArrivedAt = Now.AddMinutes(-90);
        _trip.Stops[1].ArrivedAt = Now.AddHours(-5);
        _trip.Stops[1].DepartedAt = Now.AddHours(-1);

        var sent = await _sut.SendAlertsAsync(Now);

        Assert.Equal(0, sent);
        await _tenantUow.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}

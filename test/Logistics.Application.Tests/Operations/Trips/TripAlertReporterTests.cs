using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.Context;
using Logistics.Application.Modules.AI.AIDispatch.Commands;
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

public class TripAlertReporterTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IEtaService _eta = Substitute.For<IEtaService>();
    private readonly ICommandEnqueuer _enqueuer = Substitute.For<ICommandEnqueuer>();
    private readonly IFeatureService _features = Substitute.For<IFeatureService>();
    private readonly Guid _driverId = Guid.NewGuid();
    private readonly Trip _trip;
    private readonly TripAlertReporter _sut;

    public TripAlertReporterTests()
    {
        var truck = new Truck { Number = "T-9", Type = TruckType.FreightTruck, MainDriverId = _driverId };
        _trip = Trip.Create("Trip", truck, [LoadInvoiceChargesTests.NewLoad()]);
        _trip.Dispatch();

        _tenantUow.Stub<Trip>(_trip);
        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());

        _features.IsFeatureEnabledAsync(Arg.Any<Guid>(), TenantFeature.AITripAlerts).Returns(true);

        _sut = new TripAlertReporter(_tenantUow, _eta, _enqueuer, _features);
    }

    [Fact]
    public async Task CheckHosShortfall_FewerHoursThanTheTripNeeds_FlagsOnceAndEnqueues()
    {
        _eta.GetRemainingDriveTime(_trip, Arg.Any<DateTime>()).Returns(TimeSpan.FromHours(6));

        await _sut.CheckHosShortfallsAsync(new Dictionary<Guid, int> { [_driverId] = 300 });
        await _sut.CheckHosShortfallsAsync(new Dictionary<Guid, int> { [_driverId] = 290 });

        Assert.NotNull(_trip.HosShortfallFlaggedAt);
        _enqueuer.Received(1).Enqueue(Arg.Is<RaiseTripAlertCommand>(c =>
            c.TripId == _trip.Id && c.Kind == TripAlertKind.HosShortfall && c.Details.Contains("1h 00m short")));
    }

    [Fact]
    public async Task CheckHosShortfall_EnoughHours_DoesNothing()
    {
        _eta.GetRemainingDriveTime(_trip, Arg.Any<DateTime>()).Returns(TimeSpan.FromHours(4));

        await _sut.CheckHosShortfallsAsync(new Dictionary<Guid, int> { [_driverId] = 300 });

        Assert.Null(_trip.HosShortfallFlaggedAt);
        _enqueuer.DidNotReceiveWithAnyArgs().Enqueue(default!);
    }

    [Fact]
    public async Task CheckHosShortfall_NoEtaYet_DoesNothing()
    {
        _eta.GetRemainingDriveTime(_trip, Arg.Any<DateTime>()).Returns((TimeSpan?)null);

        await _sut.CheckHosShortfallsAsync(new Dictionary<Guid, int> { [_driverId] = 10 });

        _enqueuer.DidNotReceiveWithAnyArgs().Enqueue(default!);
    }

    [Fact]
    public async Task ReportViolations_DriverWithoutActiveTrip_DoesNothing()
    {
        await _sut.ReportViolationsAsync(Guid.NewGuid(), newViolations: 2);

        _enqueuer.DidNotReceiveWithAnyArgs().Enqueue(default!);
    }
}

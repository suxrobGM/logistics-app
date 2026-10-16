using System.Linq.Expressions;
using Logistics.Application.Abstractions.Eld;
using Logistics.Application.Abstractions.Realtime;
using Logistics.Application.Modules.Compliance.Eld.Services;
using Logistics.Application.Modules.Compliance.Ifta.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Eld;

public class EldSyncServiceTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IEldProviderFactory _factory = Substitute.For<IEldProviderFactory>();
    private readonly IEldProviderService _provider = Substitute.For<IEldProviderService, IEldGpsTrackingProvider>();
    private readonly ITruckLocationRecorder _recorder = Substitute.For<ITruckLocationRecorder>();
    private readonly IRealtimeLiveTrackingService _liveTracking = Substitute.For<IRealtimeLiveTrackingService>();

    private readonly ITenantRepository<EldDriverMapping, Guid> _driverMappings = Substitute.For<ITenantRepository<EldDriverMapping, Guid>>();
    private readonly ITenantRepository<EldVehicleMapping, Guid> _vehicleMappings = Substitute.For<ITenantRepository<EldVehicleMapping, Guid>>();
    private readonly ITenantRepository<DriverHosStatus, Guid> _statuses = Substitute.For<ITenantRepository<DriverHosStatus, Guid>>();
    private readonly ITenantRepository<HosLog, Guid> _logs = Substitute.For<ITenantRepository<HosLog, Guid>>();
    private readonly ITenantRepository<HosViolation, Guid> _violations = Substitute.For<ITenantRepository<HosViolation, Guid>>();
    private readonly ITenantRepository<Truck, Guid> _trucks = Substitute.For<ITenantRepository<Truck, Guid>>();

    private readonly EldProviderConfiguration _config = new() { ProviderType = EldProviderType.Samsara, ApiKey = "token" };
    private readonly EldDriverMapping _mapping;
    private readonly EldSyncService _sut;

    public EldSyncServiceTests()
    {
        _mapping = new EldDriverMapping
        {
            EmployeeId = Guid.NewGuid(),
            ProviderType = EldProviderType.Samsara,
            ExternalDriverId = "88668"
        };

        _tenantUow.GetCurrentTenant().Returns(TestTenant.Create());
        _tenantUow.Repository<EldDriverMapping>().Returns(_driverMappings);
        _tenantUow.Repository<EldVehicleMapping>().Returns(_vehicleMappings);
        _tenantUow.Repository<DriverHosStatus>().Returns(_statuses);
        _tenantUow.Repository<HosLog>().Returns(_logs);
        _tenantUow.Repository<HosViolation>().Returns(_violations);
        _tenantUow.Repository<Truck>().Returns(_trucks);

        _driverMappings.GetListAsync(Any<EldDriverMapping>(), Arg.Any<CancellationToken>()).Returns([_mapping]);
        _vehicleMappings.GetListAsync(Any<EldVehicleMapping>(), Arg.Any<CancellationToken>()).Returns([]);
        _statuses.GetListAsync(Any<DriverHosStatus>(), Arg.Any<CancellationToken>()).Returns([]);
        _logs.GetListAsync(Any<HosLog>(), Arg.Any<CancellationToken>()).Returns([]);
        _violations.GetListAsync(Any<HosViolation>(), Arg.Any<CancellationToken>()).Returns([]);
        ((IEldGpsTrackingProvider)_provider).GetAllVehicleLocationsAsync(Arg.Any<CancellationToken>()).Returns([]);

        _factory.GetProvider(_config).Returns(_provider);

        _sut = new EldSyncService(_tenantUow, _factory, _recorder, _liveTracking, NullLogger<EldSyncService>.Instance);
    }

    #region HOS clocks

    [Fact]
    public async Task SyncLiveAsync_DuplicateAndEmptyDriverIds_CreatesOneStatusFromFirstClock()
    {
        _provider.GetAllDriversHosStatusAsync().Returns(
        [
            Clock("", DutyStatus.OffDuty),
            Clock("88668", DutyStatus.Driving, drivingMinutes: 300),
            Clock("88668", DutyStatus.OffDuty, drivingMinutes: 10)
        ]);

        await _sut.SyncLiveAsync(_config, CancellationToken.None);

        await _statuses.Received(1).AddAsync(
            Arg.Is<DriverHosStatus>(s =>
                s.EmployeeId == _mapping.EmployeeId &&
                s.CurrentDutyStatus == DutyStatus.Driving &&
                s.DrivingMinutesRemaining == 300),
            Arg.Any<CancellationToken>());
        Assert.NotNull(_mapping.LastSyncedAt);
        Assert.NotNull(_config.LastSyncedAt);
    }

    [Fact]
    public async Task SyncLiveAsync_StatusChangedWithoutProviderTimestamp_DatesTheChangeNow()
    {
        var existing = new DriverHosStatus
        {
            EmployeeId = _mapping.EmployeeId,
            CurrentDutyStatus = DutyStatus.OffDuty,
            StatusChangedAt = DateTime.UtcNow.AddHours(-10)
        };
        _statuses.GetListAsync(Any<DriverHosStatus>(), Arg.Any<CancellationToken>()).Returns([existing]);
        _provider.GetAllDriversHosStatusAsync().Returns([Clock("88668", DutyStatus.Driving)]);

        await _sut.SyncLiveAsync(_config, CancellationToken.None);

        Assert.Equal(DutyStatus.Driving, existing.CurrentDutyStatus);
        Assert.True(existing.StatusChangedAt > DateTime.UtcNow.AddMinutes(-1));
        await _statuses.DidNotReceive().AddAsync(Arg.Any<DriverHosStatus>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region HOS logs

    [Fact]
    public async Task SyncHistoryAsync_EditedLogs_UpdatesMatchesDeletesStaleAndAddsNew()
    {
        var start = DateTime.UtcNow.AddHours(-6);
        var kept = new HosLog { EmployeeId = _mapping.EmployeeId, LogDate = start.Date, DutyStatus = DutyStatus.OffDuty, StartTime = start, ExternalLogId = "a" };
        var stale = new HosLog { EmployeeId = _mapping.EmployeeId, LogDate = start.Date, DutyStatus = DutyStatus.OffDuty, StartTime = start, ExternalLogId = "b" };
        _logs.GetListAsync(Any<HosLog>(), Arg.Any<CancellationToken>()).Returns([kept, stale]);
        _provider.GetDriverHosLogsAsync("88668", Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(
        [
            Log("a", start, DutyStatus.Driving),
            Log("c", start.AddHours(2), DutyStatus.OnDutyNotDriving),
            Log("c", start.AddHours(2), DutyStatus.OnDutyNotDriving)
        ]);

        await _sut.SyncHistoryAsync(_config, CancellationToken.None);

        Assert.Equal(DutyStatus.Driving, kept.DutyStatus);
        _logs.Received(1).Delete(stale);
        _logs.DidNotReceive().Delete(kept);
        await _logs.Received(1).AddAsync(
            Arg.Is<HosLog>(l => l.ExternalLogId == "c" && l.ProviderType == EldProviderType.Samsara),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task SyncHistoryAsync_ProviderReturnsNoLogs_KeepsStoredHistory()
    {
        _provider.GetDriverHosLogsAsync("88668", Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);

        await _sut.SyncHistoryAsync(_config, CancellationToken.None);

        await _logs.DidNotReceive().GetListAsync(Any<HosLog>(), Arg.Any<CancellationToken>());
        _logs.DidNotReceive().Delete(Arg.Any<HosLog>());
    }

    [Fact]
    public async Task SyncHistoryAsync_LogBeforeWindow_IsNotStored()
    {
        _provider.GetDriverHosLogsAsync("88668", Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(
            [Log("old", DateTime.UtcNow.AddDays(-9), DutyStatus.OffDuty)]);

        await _sut.SyncHistoryAsync(_config, CancellationToken.None);

        await _logs.DidNotReceive().AddAsync(Arg.Any<HosLog>(), Arg.Any<CancellationToken>());
    }

    #endregion

    #region Violations

    [Fact]
    public async Task SyncHistoryAsync_KnownViolation_IsNotAddedAgain()
    {
        _provider.GetDriverHosLogsAsync("88668", Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns([]);
        _provider.GetDriverViolationsAsync("88668", Arg.Any<DateTime>(), Arg.Any<DateTime>()).Returns(
            [Violation("known"), Violation("new"), Violation("new")]);
        _violations.GetListAsync(Any<HosViolation>(), Arg.Any<CancellationToken>()).Returns(
        [
            new HosViolation
            {
                EmployeeId = _mapping.EmployeeId,
                ViolationDate = DateTime.UtcNow,
                ViolationType = HosViolationType.Break30Minute,
                Description = "Missed break",
                ExternalViolationId = "known"
            }
        ]);

        await _sut.SyncHistoryAsync(_config, CancellationToken.None);

        await _violations.Received(1).AddAsync(
            Arg.Is<HosViolation>(v => v.ExternalViolationId == "new" && v.RuleSetCode == "FMCSA"),
            Arg.Any<CancellationToken>());
    }

    #endregion

    #region GPS

    [Fact]
    public async Task SyncLiveAsync_GpsProvider_RecordsMappedTruckWithOdometerAndBroadcasts()
    {
        _provider.GetAllDriversHosStatusAsync().Returns([]);
        var truck = new Truck { Number = "T-7", Type = TruckType.FreightTruck };
        _vehicleMappings.GetListAsync(Any<EldVehicleMapping>(), Arg.Any<CancellationToken>()).Returns(
            [new EldVehicleMapping { TruckId = truck.Id, ProviderConfigurationId = _config.Id, ProviderType = EldProviderType.Samsara, ExternalVehicleId = "112" }]);
        _trucks.GetListAsync(Any<Truck>(), Arg.Any<CancellationToken>()).Returns([truck]);
        var seen = new DateTime(2026, 10, 14, 12, 0, 0, DateTimeKind.Utc);
        ((IEldGpsTrackingProvider)_provider).GetAllVehicleLocationsAsync(Arg.Any<CancellationToken>()).Returns(
        [
            new EldVehicleLocationDto { ExternalVehicleId = "112", Latitude = 41.88, Longitude = -87.63, Timestamp = seen, OdometerMiles = 8705 },
            new EldVehicleLocationDto { ExternalVehicleId = "999", Latitude = 40, Longitude = -80, Timestamp = seen }
        ]);

        await _sut.SyncLiveAsync(_config, CancellationToken.None);

        await _recorder.Received(1).RecordAsync(
            truck, new GeoPoint(-87.63, 41.88), seen, EldProviderType.Samsara, 8705, Arg.Any<CancellationToken>());
        await _liveTracking.Received(1).BroadcastGeolocationDataAsync(
            Arg.Any<string>(), Arg.Is<TruckGeolocationDto>(g => g.TruckId == truck.Id), Arg.Any<CancellationToken>());
    }

    #endregion

    private static Expression<Func<T, bool>> Any<T>() => Arg.Any<Expression<Func<T, bool>>>();

    private static EldDriverHosDataDto Clock(string driverId, DutyStatus status, int drivingMinutes = 0) => new()
    {
        ExternalDriverId = driverId,
        CurrentDutyStatus = status,
        DrivingMinutesRemaining = drivingMinutes
    };

    private static EldHosLogEntryDto Log(string id, DateTime start, DutyStatus status) => new()
    {
        ExternalLogId = id,
        ExternalDriverId = "88668",
        LogDate = start.Date,
        DutyStatus = status,
        StartTime = start,
        EndTime = start.AddHours(1),
        DurationMinutes = 60
    };

    private static EldViolationDataDto Violation(string id) => new()
    {
        ExternalViolationId = id,
        ExternalDriverId = "88668",
        ViolationDate = DateTime.UtcNow.AddDays(-1),
        ViolationType = HosViolationType.Break30Minute,
        Description = "Missed break",
        SeverityLevel = 3
    };
}

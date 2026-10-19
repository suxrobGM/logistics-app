using Logistics.Application.Abstractions.Geocoding;
using Logistics.Application.Modules.Compliance.Ifta.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Ifta;

public class TruckLocationRecorderTests
{
    private static readonly GeoPoint Dallas = new(-96.797, 32.7767);
    private static readonly GeoPoint FortWorth = new(-97.3308, 32.7555); // ~31 mi from Dallas
    private static readonly GeoPoint Chicago = new(-87.6298, 41.8781);   // ~800 mi from Dallas

    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IJurisdictionResolver _resolver = Substitute.For<IJurisdictionResolver>();

    private readonly ITenantRepository<TruckLocationHistory, Guid> _historyRepo =
        Substitute.For<ITenantRepository<TruckLocationHistory, Guid>>();

    private readonly Truck _truck = new() { Number = "T-100", Type = TruckType.FreightTruck };
    private readonly TruckLocationRecorder _sut;
    private FormattableString? _upsert;

    public TruckLocationRecorderTests()
    {
        _tenantUow.Repository<TruckLocationHistory>().Returns(_historyRepo);
        _tenantUow.ExecuteSqlAsync(Arg.Do<FormattableString>(sql => _upsert = sql), Arg.Any<CancellationToken>())
            .Returns(1);

        SetupPrevious(null);
        _resolver.Resolve(Arg.Any<GeoPoint>())
            .Returns(new TaxJurisdiction { CountryCode = "US", Region = "TX" });

        _sut = new TruckLocationRecorder(_tenantUow, _resolver, NullLogger<TruckLocationRecorder>.Instance);
    }

    private decimal AccruedMiles => (decimal)_upsert!.GetArgument(3)!;
    private string? AccruedRegion => (string?)_upsert!.GetArgument(5);

    private void SetupPrevious(TruckLocationHistory? previous)
    {
        var rows = previous is null ? new List<TruckLocationHistory>() : [previous];
        _historyRepo.Query().Returns(rows.BuildMock());
    }

    private TruckLocationHistory Previous(GeoPoint location, DateTime timestamp, int? odometer = null) => new()
    {
        TruckId = _truck.Id,
        Location = location,
        Timestamp = timestamp,
        OdometerReading = odometer
    };

    [Fact]
    public async Task Record_FirstPing_AppendsHistoryWithoutAccrual()
    {
        await _sut.RecordAsync(_truck, Dallas, DateTime.UtcNow);

        await _historyRepo.Received(1).AddAsync(Arg.Any<TruckLocationHistory>(), Arg.Any<CancellationToken>());
        Assert.Null(_upsert);
        Assert.Equal(Dallas, _truck.CurrentLocation);
    }

    [Fact]
    public async Task Record_NormalSegment_UpsertsJurisdictionMiles()
    {
        var now = DateTime.UtcNow;
        SetupPrevious(Previous(Dallas, now.AddHours(-1)));

        await _sut.RecordAsync(_truck, FortWorth, now);

        Assert.NotNull(_upsert);
        Assert.Equal(_truck.Id, _upsert.GetArgument(1));
        Assert.Equal(DateOnly.FromDateTime(now), _upsert.GetArgument(2));
        Assert.InRange(AccruedMiles, 25m, 40m);
        Assert.Equal("US", _upsert.GetArgument(4));
        Assert.Equal("TX", AccruedRegion);
    }

    [Fact]
    public async Task Record_NormalSegment_AddsToAnExistingRowAtomically()
    {
        var now = DateTime.UtcNow;
        SetupPrevious(Previous(Dallas, now.AddHours(-1)));

        await _sut.RecordAsync(_truck, FortWorth, now);

        var sql = _upsert!.Format;
        Assert.Contains("ON CONFLICT (truck_id, date, jurisdiction_country_code, jurisdiction_region)", sql);
        Assert.Contains("miles = truck_jurisdiction_mileage.miles + excluded.miles", sql);
    }

    [Fact]
    public async Task Record_Teleport_DiscardsSegmentButKeepsBreadcrumb()
    {
        var now = DateTime.UtcNow;
        // Dallas → Chicago (~800 mi) in 5 minutes is a GPS glitch
        SetupPrevious(Previous(Dallas, now.AddMinutes(-5)));

        await _sut.RecordAsync(_truck, Chicago, now);

        await _historyRepo.Received(1).AddAsync(Arg.Any<TruckLocationHistory>(), Arg.Any<CancellationToken>());
        Assert.Null(_upsert);
    }

    [Fact]
    public async Task Record_LongGap_StillAccrues()
    {
        var now = DateTime.UtcNow;
        // Dallas → Chicago over 20 hours (~40 mph) is plausible and must accrue
        SetupPrevious(Previous(Dallas, now.AddHours(-20)));

        await _sut.RecordAsync(_truck, Chicago, now);

        Assert.True(AccruedMiles > 700);
    }

    [Fact]
    public async Task Record_OutOfOrderPing_IsSkipped()
    {
        var now = DateTime.UtcNow;
        SetupPrevious(Previous(Dallas, now));

        await _sut.RecordAsync(_truck, FortWorth, now.AddMinutes(-10));

        await _historyRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        Assert.Null(_upsert);
    }

    [Fact]
    public async Task Record_PlausibleOdometerDelta_WinsOverHaversine()
    {
        var now = DateTime.UtcNow;
        // Haversine Dallas→Fort Worth ~31 mi; odometer says 40 (road distance) - within 2×h+5
        SetupPrevious(Previous(Dallas, now.AddHours(-1), odometer: 10_000));

        await _sut.RecordAsync(_truck, FortWorth, now, odometerReading: 10_040);

        Assert.Equal(40m, AccruedMiles);
    }

    [Fact]
    public async Task Record_UnresolvablePoint_FallsBackToPreviousJurisdiction()
    {
        var now = DateTime.UtcNow;
        SetupPrevious(Previous(Dallas, now.AddHours(-1)));
        _resolver.Resolve(FortWorth).Returns((TaxJurisdiction?)null);
        _resolver.Resolve(Dallas).Returns(new TaxJurisdiction { CountryCode = "US", Region = "TX" });

        await _sut.RecordAsync(_truck, FortWorth, now);

        Assert.Equal("TX", AccruedRegion);
    }

    #region Current address

    private static readonly Address DallasAddress = new()
    {
        Line1 = "1 Main St", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US"
    };

    [Fact]
    public async Task Record_MovedWithoutAddress_ClearsStaleAddress()
    {
        _truck.CurrentLocation = Dallas;
        _truck.CurrentAddress = DallasAddress;

        await _sut.RecordAsync(_truck, FortWorth, DateTime.UtcNow);

        Assert.Null(_truck.CurrentAddress);
    }

    [Fact]
    public async Task Record_JitterWithoutAddress_KeepsAddress()
    {
        _truck.CurrentLocation = Dallas;
        _truck.CurrentAddress = DallasAddress;

        await _sut.RecordAsync(_truck, new GeoPoint(Dallas.Longitude + 0.0005, Dallas.Latitude), DateTime.UtcNow);

        Assert.Same(DallasAddress, _truck.CurrentAddress);
    }

    [Fact]
    public async Task Record_WithAddress_ReplacesAddress()
    {
        _truck.CurrentLocation = FortWorth;

        await _sut.RecordAsync(_truck, Dallas, DateTime.UtcNow, address: DallasAddress);

        Assert.Same(DallasAddress, _truck.CurrentAddress);
    }

    #endregion
}

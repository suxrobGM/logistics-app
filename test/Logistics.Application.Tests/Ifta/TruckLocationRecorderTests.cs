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

    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly IJurisdictionResolver resolver = Substitute.For<IJurisdictionResolver>();

    private readonly ITenantRepository<TruckLocationHistory, Guid> historyRepo =
        Substitute.For<ITenantRepository<TruckLocationHistory, Guid>>();
    private readonly ITenantRepository<TruckJurisdictionMileage, Guid> rollupRepo =
        Substitute.For<ITenantRepository<TruckJurisdictionMileage, Guid>>();

    private readonly Truck truck = new() { Number = "T-100", Type = TruckType.FreightTruck };
    private readonly TruckLocationRecorder sut;

    public TruckLocationRecorderTests()
    {
        tenantUow.Repository<TruckLocationHistory>().Returns(historyRepo);
        tenantUow.Repository<TruckJurisdictionMileage>().Returns(rollupRepo);

        SetupPrevious(null);
        rollupRepo.GetAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<TruckJurisdictionMileage, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns((TruckJurisdictionMileage?)null);
        resolver.Resolve(Arg.Any<GeoPoint>())
            .Returns(new TaxJurisdiction { CountryCode = "US", Region = "TX" });

        sut = new TruckLocationRecorder(tenantUow, resolver, NullLogger<TruckLocationRecorder>.Instance);
    }

    private void SetupPrevious(TruckLocationHistory? previous)
    {
        var rows = previous is null ? new List<TruckLocationHistory>() : [previous];
        historyRepo.Query().Returns(rows.BuildMock());
    }

    private TruckLocationHistory Previous(GeoPoint location, DateTime timestamp, int? odometer = null) => new()
    {
        TruckId = truck.Id,
        Location = location,
        Timestamp = timestamp,
        OdometerReading = odometer
    };

    [Fact]
    public async Task Record_FirstPing_AppendsHistoryWithoutAccrual()
    {
        await sut.RecordAsync(truck, Dallas, DateTime.UtcNow);

        await historyRepo.Received(1).AddAsync(Arg.Any<TruckLocationHistory>(), Arg.Any<CancellationToken>());
        await rollupRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        Assert.Equal(Dallas, truck.CurrentLocation);
    }

    [Fact]
    public async Task Record_NormalSegment_AccruesJurisdictionMiles()
    {
        var now = DateTime.UtcNow;
        SetupPrevious(Previous(Dallas, now.AddHours(-1)));

        await sut.RecordAsync(truck, FortWorth, now);

        await rollupRepo.Received(1).AddAsync(
            Arg.Is<TruckJurisdictionMileage>(m =>
                m.TruckId == truck.Id
                && m.Jurisdiction.Region == "TX"
                && m.Miles > 25 && m.Miles < 40),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Record_ExistingRollup_AccumulatesMiles()
    {
        var now = DateTime.UtcNow;
        SetupPrevious(Previous(Dallas, now.AddHours(-1)));
        var rollup = new TruckJurisdictionMileage
        {
            TruckId = truck.Id,
            Jurisdiction = new TaxJurisdiction { CountryCode = "US", Region = "TX" },
            Date = DateOnly.FromDateTime(now),
            Miles = 100
        };
        rollupRepo.GetAsync(
                Arg.Any<System.Linq.Expressions.Expression<Func<TruckJurisdictionMileage, bool>>>(),
                Arg.Any<CancellationToken>())
            .Returns(rollup);

        await sut.RecordAsync(truck, FortWorth, now);

        Assert.True(rollup.Miles > 125);
        await rollupRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Record_Teleport_DiscardsSegmentButKeepsBreadcrumb()
    {
        var now = DateTime.UtcNow;
        // Dallas → Chicago (~800 mi) in 5 minutes is a GPS glitch
        SetupPrevious(Previous(Dallas, now.AddMinutes(-5)));

        await sut.RecordAsync(truck, Chicago, now);

        await historyRepo.Received(1).AddAsync(Arg.Any<TruckLocationHistory>(), Arg.Any<CancellationToken>());
        await rollupRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Record_LongGap_StillAccrues()
    {
        var now = DateTime.UtcNow;
        // Dallas → Chicago over 20 hours (~40 mph) is plausible and must accrue
        SetupPrevious(Previous(Dallas, now.AddHours(-20)));

        await sut.RecordAsync(truck, Chicago, now);

        await rollupRepo.Received(1).AddAsync(
            Arg.Is<TruckJurisdictionMileage>(m => m.Miles > 700),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Record_OutOfOrderPing_IsSkipped()
    {
        var now = DateTime.UtcNow;
        SetupPrevious(Previous(Dallas, now));

        await sut.RecordAsync(truck, FortWorth, now.AddMinutes(-10));

        await historyRepo.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
    }

    [Fact]
    public async Task Record_PlausibleOdometerDelta_WinsOverHaversine()
    {
        var now = DateTime.UtcNow;
        // Haversine Dallas→Fort Worth ~31 mi; odometer says 40 (road distance) — within 2×h+5
        SetupPrevious(Previous(Dallas, now.AddHours(-1), odometer: 10_000));

        await sut.RecordAsync(truck, FortWorth, now, odometerReading: 10_040);

        await rollupRepo.Received(1).AddAsync(
            Arg.Is<TruckJurisdictionMileage>(m => m.Miles == 40),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Record_UnresolvablePoint_FallsBackToPreviousJurisdiction()
    {
        var now = DateTime.UtcNow;
        SetupPrevious(Previous(Dallas, now.AddHours(-1)));
        resolver.Resolve(FortWorth).Returns((TaxJurisdiction?)null);
        resolver.Resolve(Dallas).Returns(new TaxJurisdiction { CountryCode = "US", Region = "TX" });

        await sut.RecordAsync(truck, FortWorth, now);

        await rollupRepo.Received(1).AddAsync(
            Arg.Is<TruckJurisdictionMileage>(m => m.Jurisdiction.Region == "TX"),
            Arg.Any<CancellationToken>());
    }
}

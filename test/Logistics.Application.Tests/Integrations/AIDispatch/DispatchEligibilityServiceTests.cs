using Logistics.Application.Abstractions.Dispatch;
using Logistics.Application.Modules.Integrations.AIDispatch.Services;
using Logistics.Application.Modules.Operations.Loads.Services;
using Logistics.Application.Tests.TestKit;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Integrations.AIDispatch;

public class DispatchEligibilityServiceTests
{
    private readonly ITenantUnitOfWork _tenantUow = Substitute.For<ITenantUnitOfWork>();

    private readonly IVehicleCapacityChecker _capacityChecker = Substitute.For<IVehicleCapacityChecker>();

    private readonly DispatchEligibilityService _sut;

    private static readonly Guid DriverId = Guid.NewGuid();
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    public DispatchEligibilityServiceTests()
    {
        _sut = new DispatchEligibilityService(
            _tenantUow, _capacityChecker, NullLogger<DispatchEligibilityService>.Instance);
    }

    #region Scenario table

    /// <summary>
    /// One driver, truck and load per row. <see cref="Expected"/> empty means eligible with no
    /// issues at all; on an eligible row a code is a warning, on a blocked row the result must carry
    /// one of the codes.
    /// </summary>
    private sealed record Scenario(
        DriverLicense[] Licenses,
        bool Eligible,
        EligibilityIssueCode[] Expected,
        HazmatClass? Hazmat = null,
        string Origin = "US",
        string Destination = "US",
        bool Placarded = false,
        AdrEquipment? Adr = null,
        bool TruckHasNoMainDriver = false,
        bool PassDriverId = false,
        EmployeeStatus DriverStatus = EmployeeStatus.Active);

    private static readonly Dictionary<string, Scenario> Scenarios = new()
    {
        ["US CDL, ordinary freight on an EU route"] = new(
            [License("US", LicenseClass.USClassA)], true, [], Origin: "DE", Destination: "FR"),
        ["US hazmat, no hazmat endorsement"] = new(
            [License("US", LicenseClass.USClassA)], false, [EligibilityIssueCode.MissingHazmatEndorsement],
            HazmatClass.Class3, Placarded: true),
        ["US hazmat, endorsed driver, truck not placarded"] = new(
            [License("US", LicenseClass.USClassA, LicenseEndorsement.Hazmat)], false,
            [EligibilityIssueCode.HazmatPlacardingRequired], HazmatClass.Class3),
        ["US hazmat, endorsed driver, placarded truck without ADR equipment"] = new(
            [License("US", LicenseClass.USClassA, LicenseEndorsement.Hazmat)], true, [],
            HazmatClass.Class3, Placarded: true),
        ["US hazmat, EU licence only, so the ADR rules apply"] = new(
            [License("DE", LicenseClass.EUCE)], false, [EligibilityIssueCode.MissingAdrEndorsement],
            HazmatClass.Class3, Placarded: true),
        ["US hazmat, EU and US licences, the US one is picked"] = new(
            [License("DE", LicenseClass.EUCE, LicenseEndorsement.Adr), License("US", LicenseClass.USClassA, LicenseEndorsement.Hazmat)],
            true, [], HazmatClass.Class3, Placarded: true),
        ["EU class 7 with basic ADR only"] = new(
            [License("DE", LicenseClass.EUCE, LicenseEndorsement.Adr)], false,
            [EligibilityIssueCode.MissingAdrClass7Endorsement], HazmatClass.Class7, "DE", "FR",
            Adr: AdrTruck(HazmatClassFlags.Class7, DateTime.UtcNow.AddYears(1))),
        ["EU truck not certified for the class"] = new(
            [License("DE", LicenseClass.EUCE, LicenseEndorsement.Adr)], false,
            [EligibilityIssueCode.TruckClassNotAllowed], HazmatClass.Class6, "DE", "FR",
            Adr: AdrTruck(HazmatClassFlags.Class3, DateTime.UtcNow.AddYears(1))),
        ["EU truck ADR certificate expired"] = new(
            [License("DE", LicenseClass.EUCE, LicenseEndorsement.Adr)], false,
            [EligibilityIssueCode.AdrCertExpired], HazmatClass.Class3, "DE", "FR",
            Adr: AdrTruck(HazmatClassFlags.Class3, DateTime.UtcNow.AddDays(-1))),
        // The expired licence may be filtered out (no active licence) or kept and flagged; both block.
        ["licence expired yesterday"] = new(
            [License("US", LicenseClass.USClassA, expiresAt: Today.AddDays(-1))], false,
            [EligibilityIssueCode.LicenseExpired, EligibilityIssueCode.NoActiveLicense]),
        ["licence expires today"] = new(
            [License("US", LicenseClass.USClassA, expiresAt: Today)], true, []),
        ["medical certificate expired"] = new(
            [License("US", LicenseClass.USClassA, medicalCertExpiresAt: Today.AddDays(-1))], false,
            [EligibilityIssueCode.MedicalCertExpired]),
        ["medical certificate expires within 7 days"] = new(
            [License("US", LicenseClass.USClassA, medicalCertExpiresAt: Today.AddDays(3))], true,
            [EligibilityIssueCode.MedicalCertExpiringSoon]),
        ["US class A CDL"] = new([License("US", LicenseClass.USClassA)], true, []),
        ["US class B CDL"] = new([License("US", LicenseClass.USClassB)], true, []),
        ["US class C CDL"] = new([License("US", LicenseClass.USClassC)], true, []),
        ["truck has no main driver and none is given"] = new(
            [License("US", LicenseClass.USClassA)], false, [EligibilityIssueCode.DriverNotAssigned],
            TruckHasNoMainDriver: true),
        ["truck has no main driver, but one is given"] = new(
            [License("US", LicenseClass.USClassA, LicenseEndorsement.Hazmat)], true, [],
            HazmatClass.Class3, Placarded: true, TruckHasNoMainDriver: true, PassDriverId: true),
        ["driver suspended"] = new(
            [License("US", LicenseClass.USClassA)], false, [EligibilityIssueCode.DriverNotActive],
            DriverStatus: EmployeeStatus.Suspended),
        ["driver terminated"] = new(
            [License("US", LicenseClass.USClassA)], false, [EligibilityIssueCode.DriverNotActive],
            DriverStatus: EmployeeStatus.Terminated),
        ["driver on leave"] = new(
            [License("US", LicenseClass.USClassA)], false, [EligibilityIssueCode.DriverNotActive],
            DriverStatus: EmployeeStatus.OnLeave),
    };

    public static TheoryData<string> ScenarioNames => [.. Scenarios.Keys];

    [Theory]
    [MemberData(nameof(ScenarioNames))]
    public async Task CheckAsync_Scenario_GivesTheExpectedVerdict(string name)
    {
        var scenario = Scenarios[name];
        var driver = TestData.Driver(DriverId, scenario.Licenses);
        driver.Status = scenario.DriverStatus;
        var truck = TestData.Truck(driver);
        truck.IsHazmatPlacarded = scenario.Placarded;
        if (scenario.Adr is not null)
        {
            truck.AdrEquipment = scenario.Adr;
        }

        if (scenario.TruckHasNoMainDriver)
        {
            truck.MainDriver = null;
            truck.MainDriverId = null;
        }

        var load = TestData.Load(originCountry: scenario.Origin, destinationCountry: scenario.Destination);
        load.HazmatClass = scenario.Hazmat;
        _tenantUow.Stub(truck);
        _tenantUow.Stub(load);
        _tenantUow.Stub(driver);

        var result = scenario.PassDriverId
            ? await _sut.CheckAsync(truck.Id, load.Id, DriverId)
            : await _sut.CheckAsync(truck.Id, load.Id);

        Assert.Equal(scenario.Eligible, result.IsEligible);
        if (scenario.Expected.Length == 0)
        {
            Assert.Empty(result.Issues);
        }
        else if (scenario.Eligible)
        {
            var warning = Assert.Single(result.Issues, i => scenario.Expected.Contains(i.Code));
            Assert.Equal(EligibilitySeverity.Warning, warning.Severity);
        }
        else
        {
            Assert.Contains(result.Issues, i => scenario.Expected.Contains(i.Code));
        }
    }

    #endregion

    [Fact]
    public async Task CheckAsync_TruckNotFound_BlockedWithReason()
    {
        _tenantUow.Stub<Truck>();

        var result = await _sut.CheckAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result.IsEligible);
        Assert.Equal(EligibilityIssueCode.TruckNotFound, Assert.Single(result.Issues).Code);
    }

    [Fact]
    public async Task CheckAsync_VehicleLoadOverTruckCapacity_WarnsButStaysEligible()
    {
        var driver = TestData.Driver(DriverId, License("US", LicenseClass.USClassA));
        var truck = TestData.Truck(driver, TruckType.CarHauler);
        var load = TestData.Load(LoadType.Vehicle);
        _tenantUow.Stub(truck);
        _tenantUow.Stub(load);
        _tenantUow.Stub(driver);
        _capacityChecker.GetWarningAsync(truck, load, Arg.Any<CancellationToken>())
            .Returns("Truck T-1 carries up to 7 vehicles, but its open loads have 9.");

        var result = await _sut.CheckAsync(truck.Id, load.Id);

        Assert.True(result.IsEligible);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(EligibilityIssueCode.VehicleCapacityExceeded, issue.Code);
        Assert.Equal(EligibilitySeverity.Warning, issue.Severity);
    }

    [Fact]
    public async Task CheckAsync_ContainerLoadOnFreightTruck_BlockedAsIncompatible()
    {
        var driver = TestData.Driver(DriverId, License("US", LicenseClass.USClassA));
        var truck = TestData.Truck(driver);
        var load = TestData.Load(LoadType.IntermodalContainer);
        _tenantUow.Stub(truck);
        _tenantUow.Stub(load);
        _tenantUow.Stub(driver);

        var result = await _sut.CheckAsync(truck.Id, load.Id);

        Assert.False(result.IsEligible);
        var issue = Assert.Single(result.Issues);
        Assert.Equal(EligibilityIssueCode.TruckTypeIncompatible, issue.Code);
        Assert.Equal(EligibilitySeverity.Error, issue.Severity);
    }

    private static DriverLicense License(
        string country,
        LicenseClass cls,
        LicenseEndorsement endorsements = LicenseEndorsement.None,
        DateOnly? expiresAt = null,
        DateOnly? medicalCertExpiresAt = null)
    {
        return DriverLicense.Create(
            DriverId,
            "LIC-" + Guid.NewGuid().ToString("N")[..6],
            cls,
            country,
            Today.AddYears(-2),
            expiresAt ?? Today.AddYears(2),
            endorsements,
            medicalCertExpiresAt: medicalCertExpiresAt);
    }

    private static AdrEquipment AdrTruck(HazmatClassFlags allowedClasses, DateTime certExpiry) => new()
    {
        IsAdrCertified = true,
        AdrCertExpiresAt = certExpiry,
        AllowedClasses = allowedClasses,
        OrangePlateNumber = "30/1203"
    };
}

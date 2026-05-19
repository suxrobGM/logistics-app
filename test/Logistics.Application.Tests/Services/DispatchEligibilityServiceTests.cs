using Logistics.Application.Modules.Integrations.AiDispatch.Services;
using Logistics.Application.Abstractions.Dispatch;
using Logistics.Domain.Entities;
using Logistics.Domain.Persistence;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace Logistics.Application.Tests.Services;

public class DispatchEligibilityServiceTests
{
    private readonly ITenantUnitOfWork tenantUow = Substitute.For<ITenantUnitOfWork>();
    private readonly ITenantRepository<Truck, Guid> truckRepo = Substitute.For<ITenantRepository<Truck, Guid>>();
    private readonly ITenantRepository<Load, Guid> loadRepo = Substitute.For<ITenantRepository<Load, Guid>>();
    private readonly ITenantRepository<Employee, Guid> employeeRepo = Substitute.For<ITenantRepository<Employee, Guid>>();

    private readonly DispatchEligibilityService sut;

    private static readonly Guid TruckId = Guid.NewGuid();
    private static readonly Guid DriverId = Guid.NewGuid();
    private static readonly Guid LoadId = Guid.NewGuid();
    private static readonly Guid CustomerId = Guid.NewGuid();

    public DispatchEligibilityServiceTests()
    {
        tenantUow.Repository<Truck>().Returns(truckRepo);
        tenantUow.Repository<Load>().Returns(loadRepo);
        tenantUow.Repository<Employee>().Returns(employeeRepo);
        sut = new DispatchEligibilityService(tenantUow, NullLogger<DispatchEligibilityService>.Instance);
    }

    [Fact]
    public async Task CheckAsync_NonHazmatLoad_ValidUsCdl_ReturnsEligible()
    {
        var driver = CreateDriver(license: BuildLicense("US", LicenseClass.UsClassA));
        var truck = CreateTruck(driver);
        var load = CreateLoad(isHazmat: false);

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.True(result.IsEligible);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public async Task CheckAsync_HazmatLoad_UsDriverWithoutHazmatEndorsement_Blocked()
    {
        var driver = CreateDriver(license: BuildLicense("US", LicenseClass.UsClassA));
        var truck = CreateTruck(driver, isHazmatPlacarded: true);
        var load = CreateLoad(isHazmat: true, hazmatClass: HazmatClass.Class3, originCountry: "US", destCountry: "US");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Issues, i => i.Code == EligibilityIssueCode.MissingHazmatEndorsement);
    }

    [Fact]
    public async Task CheckAsync_HazmatLoad_UsDriverWithHazmatAndPlacardedTruck_Eligible()
    {
        var driver = CreateDriver(license: BuildLicense("US", LicenseClass.UsClassA, LicenseEndorsement.Hazmat));
        var truck = CreateTruck(driver, isHazmatPlacarded: true);
        var load = CreateLoad(isHazmat: true, hazmatClass: HazmatClass.Class3, originCountry: "US", destCountry: "US");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.True(result.IsEligible);
    }

    [Fact]
    public async Task CheckAsync_AdrLoad_EuDriverWithBasicAdr_Class7Load_Blocked()
    {
        var driver = CreateDriver(license: BuildLicense("DE", LicenseClass.EuCE,
            LicenseEndorsement.Adr));
        var truck = CreateAdrTruck(driver, allowedClasses: HazmatClassFlags.Class7,
            certExpiry: DateTime.UtcNow.AddYears(1));
        var load = CreateLoad(isHazmat: true, hazmatClass: HazmatClass.Class7,
            originCountry: "DE", destCountry: "FR");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Issues, i => i.Code == EligibilityIssueCode.MissingAdrClass7Endorsement);
    }

    [Fact]
    public async Task CheckAsync_AdrLoad_EuTruckNotCertifiedForClass_Blocked()
    {
        var driver = CreateDriver(license: BuildLicense("DE", LicenseClass.EuCE,
            LicenseEndorsement.Adr));
        var truck = CreateAdrTruck(driver, allowedClasses: HazmatClassFlags.Class3,
            certExpiry: DateTime.UtcNow.AddYears(1));
        var load = CreateLoad(isHazmat: true, hazmatClass: HazmatClass.Class6,
            originCountry: "DE", destCountry: "FR");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Issues, i => i.Code == EligibilityIssueCode.TruckClassNotAllowed);
    }

    [Fact]
    public async Task CheckAsync_AdrCertExpired_Blocked()
    {
        var driver = CreateDriver(license: BuildLicense("DE", LicenseClass.EuCE,
            LicenseEndorsement.Adr));
        var truck = CreateAdrTruck(driver, allowedClasses: HazmatClassFlags.Class3,
            certExpiry: DateTime.UtcNow.AddDays(-1));
        var load = CreateLoad(isHazmat: true, hazmatClass: HazmatClass.Class3,
            originCountry: "DE", destCountry: "FR");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Issues, i => i.Code == EligibilityIssueCode.AdrCertExpired);
    }

    [Fact]
    public async Task CheckAsync_LicenseExpired_Blocked()
    {
        var driver = CreateDriver(license: BuildLicense("US", LicenseClass.UsClassA,
            expiresAt: DateTime.UtcNow.AddDays(-1),
            status: DriverLicenseStatus.Active));
        var truck = CreateTruck(driver);
        var load = CreateLoad(isHazmat: false);

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.False(result.IsEligible);
        // Either NoActiveLicense (filtered out) or LicenseExpired (kept and flagged) Ã¢â‚¬â€ both are valid blocks
        Assert.Contains(result.Issues,
            i => i.Code is EligibilityIssueCode.LicenseExpired or EligibilityIssueCode.NoActiveLicense);
    }

    [Fact]
    public async Task CheckAsync_MedicalCertExpired_Blocked()
    {
        var driver = CreateDriver(license: BuildLicense("US", LicenseClass.UsClassA,
            medicalCertExpiresAt: DateTime.UtcNow.AddDays(-1)));
        var truck = CreateTruck(driver);
        var load = CreateLoad(isHazmat: false);

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Issues, i => i.Code == EligibilityIssueCode.MedicalCertExpired);
    }

    [Fact]
    public async Task CheckAsync_TruckNotFound_BlockedWithReason()
    {
        truckRepo.GetByIdAsync(TruckId, Arg.Any<CancellationToken>()).Returns((Truck?)null);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.False(result.IsEligible);
        Assert.Single(result.Issues);
        Assert.Equal(EligibilityIssueCode.TruckNotFound, result.Issues[0].Code);
    }

    #region US-specific scenarios

    [Fact]
    public async Task CheckAsync_UsHazmatRoute_DriverHasHazmat_TruckNotPlacarded_Blocked()
    {
        var driver = CreateDriver(license: BuildLicense("US", LicenseClass.UsClassA, LicenseEndorsement.Hazmat));
        // Truck not placarded Ã¢â‚¬â€ required for US Hazmat transport.
        var truck = CreateTruck(driver, isHazmatPlacarded: false);
        var load = CreateLoad(isHazmat: true, hazmatClass: HazmatClass.Class3,
            originCountry: "US", destCountry: "US");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Issues, i => i.Code == EligibilityIssueCode.HazmatPlacardingRequired);
    }

    [Fact]
    public async Task CheckAsync_UsHazmatRoute_DoesNotRequireAdrCertOnTruck()
    {
        // US-issued license with Hazmat + a non-ADR-certified but placarded truck Ã¢â€ â€™
        // should pass even though truck.AdrEquipment.IsAdrCertified == false.
        var driver = CreateDriver(license: BuildLicense("US", LicenseClass.UsClassA, LicenseEndorsement.Hazmat));
        var truck = CreateTruck(driver, isHazmatPlacarded: true);
        // Explicitly assert no ADR equipment.
        Assert.False(truck.AdrEquipment.IsAdrCertified);

        var load = CreateLoad(isHazmat: true, hazmatClass: HazmatClass.Class3,
            originCountry: "US", destCountry: "US");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.True(result.IsEligible);
        Assert.DoesNotContain(result.Issues, i => i.Code == EligibilityIssueCode.TruckNotAdrCertified);
        Assert.DoesNotContain(result.Issues, i => i.Code == EligibilityIssueCode.AdrCertExpired);
    }

    [Theory]
    [InlineData(LicenseClass.UsClassA)]
    [InlineData(LicenseClass.UsClassB)]
    [InlineData(LicenseClass.UsClassC)]
    public async Task CheckAsync_NonHazmatUsLoad_AnyCdlClass_Eligible(LicenseClass cls)
    {
        var driver = CreateDriver(license: BuildLicense("US", cls));
        var truck = CreateTruck(driver);
        var load = CreateLoad(isHazmat: false, originCountry: "US", destCountry: "US");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.True(result.IsEligible);
    }

    [Fact]
    public async Task CheckAsync_UsRoute_TruckHasNoMainDriver_AndNoOverride_Blocked()
    {
        var driver = CreateDriver(license: BuildLicense("US", LicenseClass.UsClassA));
        var truck = CreateTruck(driver);
        truck.MainDriver = null;
        truck.MainDriverId = null;
        var load = CreateLoad(isHazmat: false, originCountry: "US", destCountry: "US");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Issues, i => i.Code == EligibilityIssueCode.DriverNotAssigned);
    }

    [Fact]
    public async Task CheckAsync_UsRoute_ExplicitDriverIdOverride_UsesProvidedDriver()
    {
        // Truck has no main driver assigned but caller passes driverId explicitly
        // (e.g., simulating "would secondary driver X be eligible?" planning).
        var driver = CreateDriver(license: BuildLicense("US", LicenseClass.UsClassA, LicenseEndorsement.Hazmat));
        var truck = CreateTruck(driver, isHazmatPlacarded: true);
        truck.MainDriver = null;
        truck.MainDriverId = null;
        var load = CreateLoad(isHazmat: true, hazmatClass: HazmatClass.Class3,
            originCountry: "US", destCountry: "US");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId, DriverId);

        Assert.True(result.IsEligible);
    }

    [Fact]
    public async Task CheckAsync_UsRoute_DriverHasOnlyEuLicense_NoUsCdl_Blocked()
    {
        // Driver only holds an EU license; load is US domestic. No US CDL on file Ã¢â€ â€™
        // primary-license picker still selects the EU license (it's the only active one),
        // and the US Hazmat-endorsement gate doesn't apply (license is non-US), so on a
        // non-hazmat US load this passes. But on a US Hazmat load, the EU branch fires
        // and demands ADR Ã¢â‚¬â€ which the truck doesn't have Ã¢â€ â€™ blocked.
        var driver = CreateDriver(license: BuildLicense("DE", LicenseClass.EuCE));
        var truck = CreateTruck(driver, isHazmatPlacarded: true);
        var load = CreateLoad(isHazmat: true, hazmatClass: HazmatClass.Class3,
            originCountry: "US", destCountry: "US");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.False(result.IsEligible);
        Assert.Contains(result.Issues, i => i.Code == EligibilityIssueCode.MissingAdrEndorsement);
    }

    [Fact]
    public async Task CheckAsync_UsRoute_MultipleLicenses_PrefersUsIssuedForUsRoute()
    {
        // Driver holds both an EU CE license (no Hazmat) and a US CDL with Hazmat.
        // For a US route + Hazmat load, the picker should prefer the US license Ã¢â‚¬â€ and so
        // the US Hazmat-endorsement gate should pass.
        var euLicense = BuildLicense("DE", LicenseClass.EuCE, LicenseEndorsement.Adr);
        var usLicense = BuildLicense("US", LicenseClass.UsClassA, LicenseEndorsement.Hazmat);
        var driver = new Employee
        {
            Id = DriverId,
            Email = "driver@test.com",
            FirstName = "John",
            LastName = "Doe",
            Licenses = [euLicense, usLicense]
        };
        var truck = CreateTruck(driver, isHazmatPlacarded: true);
        var load = CreateLoad(isHazmat: true, hazmatClass: HazmatClass.Class3,
            originCountry: "US", destCountry: "US");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        Assert.True(result.IsEligible);
    }

    [Fact]
    public async Task CheckAsync_UsCdl_MedicalCertExpiringWithin7Days_WarningNotError()
    {
        var driver = CreateDriver(license: BuildLicense("US", LicenseClass.UsClassA,
            medicalCertExpiresAt: DateTime.UtcNow.AddDays(3)));
        var truck = CreateTruck(driver);
        var load = CreateLoad(isHazmat: false, originCountry: "US", destCountry: "US");

        WireRepos(truck, load, driver);

        var result = await sut.CheckAsync(TruckId, LoadId);

        // Eligible (no Error severity), but a Warning is surfaced.
        Assert.True(result.IsEligible);
        Assert.Contains(result.Issues, i =>
            i.Code == EligibilityIssueCode.MedicalCertExpiringSoon
            && i.Severity == EligibilitySeverity.Warning);
    }

    #endregion

    private void WireRepos(Truck truck, Load load, Employee driver)
    {
        truckRepo.GetByIdAsync(TruckId, Arg.Any<CancellationToken>()).Returns(truck);
        loadRepo.GetByIdAsync(LoadId, Arg.Any<CancellationToken>()).Returns(load);
        employeeRepo.GetByIdAsync(DriverId, Arg.Any<CancellationToken>()).Returns(driver);
    }

    private static DriverLicense BuildLicense(
        string country,
        LicenseClass cls,
        LicenseEndorsement endorsements = LicenseEndorsement.None,
        DateTime? expiresAt = null,
        DateTime? medicalCertExpiresAt = null,
        DriverLicenseStatus status = DriverLicenseStatus.Active)
    {
        return new DriverLicense
        {
            EmployeeId = DriverId,
            LicenseNumber = "LIC-" + Guid.NewGuid().ToString("N")[..6],
            LicenseClass = cls,
            Endorsements = endorsements,
            IssuingCountry = country,
            IssuedDate = DateTime.UtcNow.AddYears(-2),
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddYears(2),
            MedicalCertExpiresAt = medicalCertExpiresAt,
            Status = status
        };
    }

    private static Employee CreateDriver(DriverLicense license)
    {
        return new Employee
        {
            Id = DriverId,
            Email = "driver@test.com",
            FirstName = "John",
            LastName = "Doe",
            Licenses = [license]
        };
    }

    private static Truck CreateTruck(Employee driver, bool isHazmatPlacarded = false)
    {
        return new Truck
        {
            Id = TruckId,
            Number = "TRK-001",
            Type = TruckType.FreightTruck,
            MainDriver = driver,
            MainDriverId = driver.Id,
            IsHazmatPlacarded = isHazmatPlacarded
        };
    }

    private static Truck CreateAdrTruck(Employee driver, HazmatClassFlags allowedClasses, DateTime certExpiry)
    {
        var truck = CreateTruck(driver);
        truck.AdrEquipment = new AdrEquipment
        {
            IsAdrCertified = true,
            AdrCertExpiresAt = certExpiry,
            AllowedClasses = allowedClasses,
            OrangePlateNumber = "30/1203"
        };
        return truck;
    }

    private static Load CreateLoad(
        bool isHazmat,
        HazmatClass? hazmatClass = null,
        string originCountry = "DE",
        string destCountry = "FR")
    {
        return new Load
        {
            Id = LoadId,
            Name = "Test Load",
            Type = LoadType.GeneralFreight,
            CustomerId = CustomerId,
            Customer = null!,
            OriginAddress = new Address
            {
                Line1 = "1 Origin", City = "City", State = "ST", ZipCode = "00000", Country = originCountry
            },
            OriginLocation = new GeoPoint(0, 0),
            DestinationAddress = new Address
            {
                Line1 = "1 Dest", City = "City", State = "ST", ZipCode = "00000", Country = destCountry
            },
            DestinationLocation = new GeoPoint(0, 0),
            DeliveryCost = Money.Zero("USD"),
            IsHazmat = isHazmat,
            HazmatClass = hazmatClass
        };
    }
}

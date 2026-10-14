using Logistics.Application.Modules.Compliance.Eld.Services;
using Logistics.Shared.Models;
using Xunit;

namespace Logistics.Application.Tests.Eld;

public class EldMappingMatcherTests
{
    #region Drivers

    [Fact]
    public void MatchDrivers_SameEmailDifferentCase_MatchesByEmail()
    {
        var employee = Employee("Susan Bobson", "Susan.Bob@Fleet.com");

        var match = Assert.Single(EldMappingMatcher.MatchDrivers(
            [EldDriver("88668", "Susan Bob", email: "susan.bob@fleet.com")], [employee]));

        Assert.Equal(employee.EmployeeId, match.EmployeeId);
        Assert.Equal(EldMatchReason.Email, match.MatchedBy);
    }

    [Fact]
    public void MatchDrivers_LicenseWithDifferentPunctuation_MatchesByLicense()
    {
        var employee = Employee("Bob Driver", licenses: ["B0B-DR1V"]);

        var match = Assert.Single(EldMappingMatcher.MatchDrivers(
            [EldDriver("45646", "Driver Bob", license: "b0bdr1v")], [employee]));

        Assert.Equal(EldMatchReason.LicenseNumber, match.MatchedBy);
    }

    [Fact]
    public void MatchDrivers_NameWithExtraSpacesAndCase_MatchesByName()
    {
        var match = Assert.Single(EldMappingMatcher.MatchDrivers(
            [EldDriver("1", "  maria   GARCIA ")], [Employee("Maria Garcia")]));

        Assert.Equal(EldMatchReason.Name, match.MatchedBy);
    }

    [Fact]
    public void MatchDrivers_TwoEmployeesWithTheSameName_SuggestsNothing()
    {
        var suggestions = EldMappingMatcher.MatchDrivers(
            [EldDriver("1", "John Smith")], [Employee("John Smith"), Employee("John Smith")]);

        Assert.Empty(suggestions);
    }

    [Fact]
    public void MatchDrivers_EmailMatchedFirst_EmployeeIsNotReusedForName()
    {
        var employee = Employee("John Smith", "john@fleet.com");

        var suggestions = EldMappingMatcher.MatchDrivers(
        [
            EldDriver("by-email", "J. Smith", email: "john@fleet.com"),
            EldDriver("by-name", "John Smith")
        ], [employee]);

        var match = Assert.Single(suggestions);
        Assert.Equal("by-email", match.ExternalDriverId);
    }

    #endregion

    #region Vehicles

    [Fact]
    public void MatchVehicles_VinMatches_PrefersVinOverNumber()
    {
        var byVin = Truck("200", "1FUJA6BD31LJ09646");
        var byNumber = Truck("101");

        var match = Assert.Single(EldMappingMatcher.MatchVehicles(
            [EldVehicle("112", "Truck 101", "1fuja6bd31lj09646")], [byVin, byNumber]));

        Assert.Equal(byVin.TruckId, match.TruckId);
        Assert.Equal(EldMatchReason.Vin, match.MatchedBy);
    }

    [Theory]
    [InlineData("101")]
    [InlineData("Truck #101")]
    [InlineData("T-101")]
    public void MatchVehicles_NameCarriesTruckNumber_MatchesByNumber(string vehicleName)
    {
        var match = Assert.Single(EldMappingMatcher.MatchVehicles(
            [EldVehicle("112", vehicleName)], [Truck("101")]));

        Assert.Equal(EldMatchReason.TruckNumber, match.MatchedBy);
    }

    [Fact]
    public void MatchVehicles_TwoVehiclesCarryTheSameNumber_SuggestsNothing()
    {
        var suggestions = EldMappingMatcher.MatchVehicles(
            [EldVehicle("1", "Truck 101"), EldVehicle("2", "Unit 101")], [Truck("101")]);

        Assert.Empty(suggestions);
    }

    #endregion

    private static DriverCandidate Employee(string name, string? email = null, string[]? licenses = null) =>
        new(Guid.NewGuid(), name, email, licenses ?? []);

    private static TruckCandidate Truck(string number, string? vin = null) => new(Guid.NewGuid(), number, vin);

    private static EldDriverDto EldDriver(string id, string name, string? email = null, string? license = null) => new()
    {
        ExternalDriverId = id,
        Name = name,
        Email = email,
        LicenseNumber = license
    };

    private static EldVehicleDto EldVehicle(string id, string name, string? vin = null) => new()
    {
        ExternalVehicleId = id,
        Name = name,
        Vin = vin
    };
}

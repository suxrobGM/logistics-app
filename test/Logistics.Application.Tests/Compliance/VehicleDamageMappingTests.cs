using Logistics.Application.Modules.Compliance.Inspections.Commands;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Safety;
using Xunit;

namespace Logistics.Application.Tests.Compliance;

public class VehicleDamageMappingTests
{
    [Theory]
    [InlineData(VehicleDamageType.Scratch, "S")]
    [InlineData(VehicleDamageType.Dent, "D")]
    [InlineData(VehicleDamageType.Chip, "C")]
    [InlineData(VehicleDamageType.Crack, "CR")]
    [InlineData(VehicleDamageType.Missing, "M")]
    [InlineData(VehicleDamageType.Broken, "B")]
    public void GetCode_ReturnsCarHaulerBolCode(VehicleDamageType type, string code)
    {
        Assert.Equal(code, type.GetCode());
    }

    [Theory]
    [InlineData(VehicleDamageZone.Hood, CargoInspectionPartCategory.VehicleHood)]
    [InlineData(VehicleDamageZone.LeftFrontDoor, CargoInspectionPartCategory.VehicleFrontLeftDoor)]
    [InlineData(VehicleDamageZone.RightRearDoor, CargoInspectionPartCategory.VehicleRearRightDoor)]
    [InlineData(VehicleDamageZone.RightFrontFender, CargoInspectionPartCategory.VehicleFenders)]
    [InlineData(VehicleDamageZone.LeftRearQuarterPanel, CargoInspectionPartCategory.VehicleBodyPanels)]
    [InlineData(VehicleDamageZone.RearWindow, CargoInspectionPartCategory.VehicleSideGlass)]
    [InlineData(VehicleDamageZone.RightRearWheel, CargoInspectionPartCategory.VehicleWheels)]
    [InlineData(VehicleDamageZone.Trunk, CargoInspectionPartCategory.VehicleTrunkLiftgate)]
    public void ToPartCategory_MapsZoneToItsPart(VehicleDamageZone zone, CargoInspectionPartCategory expected)
    {
        Assert.Equal(expected, zone.ToPartCategory());
    }

    [Fact]
    public void ToPartCategory_EveryZone_IsAValidVehiclePart()
    {
        foreach (var zone in Enum.GetValues<VehicleDamageZone>())
        {
            Assert.True(zone.ToPartCategory().IsValidFor(LoadType.Vehicle), $"{zone} maps outside the vehicle catalog");
        }
    }

    [Fact]
    public void GetCode_EveryType_HasACode()
    {
        foreach (var type in Enum.GetValues<VehicleDamageType>())
        {
            Assert.False(string.IsNullOrEmpty(type.GetCode()));
        }
    }

    [Fact]
    public void ResolvePartCategory_ZoneSet_UsesZoneOverSentCategory()
    {
        var defect = new ConditionDefectInput
        {
            Severity = DefectSeverity.Minor,
            PartCategory = CargoInspectionPartCategory.Other,
            Zone = VehicleDamageZone.Roof,
            DamageType = VehicleDamageType.Dent
        };

        Assert.Equal(CargoInspectionPartCategory.VehicleRoof, defect.ResolvePartCategory());
    }

    [Fact]
    public void ParseDefects_DiagramDefectJson_ReadsZoneTypeAndPhotoIndex()
    {
        var defects = ConditionDefectInput.ParseDefects(
            """[{"zone":"LeftFrontDoor","damageType":"Scratch","severity":"Major","photoIndex":0}]""");

        var defect = Assert.Single(defects);
        Assert.Equal(VehicleDamageZone.LeftFrontDoor, defect.Zone);
        Assert.Equal(VehicleDamageType.Scratch, defect.DamageType);
        Assert.Equal(DefectSeverity.Major, defect.Severity);
        Assert.Equal(0, defect.PhotoIndex);
        Assert.Null(defect.Description);
    }
}

using System.Text;
using Logistics.Application.Abstractions.Documents;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Documents.Pdf;
using Logistics.Infrastructure.Documents.Pdf.VehicleBol;
using QuestPDF.Infrastructure;
using Xunit;

namespace Logistics.Infrastructure.Documents.Tests.Pdf;

public class VehicleBolPdfServiceTests
{
    private readonly VehicleBolPdfService _sut = new();

    static VehicleBolPdfServiceTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    #region PDF generation

    [Fact]
    public void Generate_TwoVehiclesWithDamageAndSignatures_ReturnsPdf()
    {
        var pickupDefects = new List<ConditionDefect>
        {
            Defect(VehicleDamageZone.Hood, VehicleDamageType.Dent),
            Defect(VehicleDamageZone.LeftFrontDoor, VehicleDamageType.Scratch)
        };
        var deliveryDefects = new List<ConditionDefect>(pickupDefects)
        {
            Defect(VehicleDamageZone.Windshield, VehicleDamageType.Crack)
        };

        var document = Document(
        [
            new VehicleBolVehicle(
                new LoadVehicle { Vin = "1HGBH41JXMN109186", Year = 2021, Make = "Honda", Model = "Accord", Color = "Blue", Rate = 450 },
                Inspection(pickupDefects),
                Inspection(deliveryDefects)),
            new VehicleBolVehicle(
                new LoadVehicle { Make = "Ford", Model = "F-150", IsOperable = false, LotNumber = "LOT-42" },
                Inspection([]),
                null)
        ],
        pickup: new VehicleBolSignature("Jane Shipper", null, null, DateTime.UtcNow.AddDays(-2), 32.7, -96.8),
        delivery: new VehicleBolSignature(null, null, "Unattended drop at dealer lot", DateTime.UtcNow, 33.4, -112.0));

        var pdf = _sut.Generate(document);

        Assert.True(pdf.Length > 1000);
        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    [Fact]
    public void Generate_NothingInspectedOrSigned_StillRenders()
    {
        var document = Document(
            [new VehicleBolVehicle(new LoadVehicle { Vin = "1HGBH41JXMN109186" }, null, null)],
            pickup: null,
            delivery: null);

        var pdf = _sut.Generate(document);

        Assert.Equal("%PDF", Encoding.ASCII.GetString(pdf, 0, 4));
    }

    #endregion

    #region Diagram

    [Fact]
    public void CodesByZone_GroupsDistinctCodesPerZone()
    {
        var codes = VehicleDiagram.CodesByZone(
        [
            Defect(VehicleDamageZone.Hood, VehicleDamageType.Dent),
            Defect(VehicleDamageZone.Hood, VehicleDamageType.Scratch),
            Defect(VehicleDamageZone.Hood, VehicleDamageType.Dent),
            Defect(VehicleDamageZone.RightRearWheel, VehicleDamageType.Missing)
        ]);

        Assert.Equal("D,S", codes[VehicleDamageZone.Hood]);
        Assert.Equal("M", codes[VehicleDamageZone.RightRearWheel]);
        Assert.Equal(2, codes.Count);
    }

    [Fact]
    public void CodesByZone_PartListDefectWithoutZone_IsLeftOffTheDiagram()
    {
        var partDefect = new ConditionDefect
        {
            PartCategory = CargoInspectionPartCategory.VehicleInterior,
            Description = "Stain on seat",
            Severity = DefectSeverity.Minor
        };

        Assert.Empty(VehicleDiagram.CodesByZone([partDefect]));
    }

    [Fact]
    public void Zones_EveryZoneHasARectangle()
    {
        foreach (var zone in Enum.GetValues<VehicleDamageZone>())
        {
            Assert.True(VehicleDiagram.Zones.ContainsKey(zone), $"{zone} is missing from the diagram");
        }
    }

    [Fact]
    public void BuildSvg_PrintsCodeInDamagedZone()
    {
        var svg = VehicleDiagram.BuildSvg([Defect(VehicleDamageZone.Roof, VehicleDamageType.Crack)]);

        Assert.StartsWith("<svg", svg);
        Assert.Contains(">CR</text>", svg);
    }

    #endregion

    private static ConditionDefect Defect(VehicleDamageZone zone, VehicleDamageType type) => new()
    {
        PartCategory = zone.ToPartCategory(),
        Description = string.Empty,
        Severity = DefectSeverity.Minor,
        Zone = zone,
        DamageType = type
    };

    private static VehicleBolInspection Inspection(List<ConditionDefect> defects) =>
        new(DateTime.UtcNow, "Driver Dan", 32.77, -96.79, "Dusty", defects);

    private static VehicleBolDocument Document(
        List<VehicleBolVehicle> vehicles,
        VehicleBolSignature? pickup,
        VehicleBolSignature? delivery) => new(
        new Tenant
        {
            Name = "acme",
            CompanyName = "Acme Auto Transport",
            CompanyAddress = Address(),
            DatabaseName = "test",
            BillingEmail = "billing@acme.test",
            DotNumber = "1234567"
        },
        1042,
        "2 vehicles",
        "Dealer Group",
        Address(),
        Address(),
        "T-7",
        "Driver Dan",
        "USD",
        vehicles,
        pickup,
        delivery,
        DateTime.UtcNow);

    private static Address Address() => new()
    {
        Line1 = "100 Main St",
        City = "Dallas",
        State = "TX",
        ZipCode = "75201",
        Country = "US"
    };
}

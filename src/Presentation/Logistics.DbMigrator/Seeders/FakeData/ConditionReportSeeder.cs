using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Regions;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Identity.Roles;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds sample load condition reports (cargo inspection — pickup/delivery).
/// Defects are drawn from the per-load-type catalog so vehicle loads get
/// vehicle-part defects, container loads get container-part defects, and
/// generic freight loads get generic-part defects.
/// </summary>
internal class ConditionReportSeeder(ILogger<ConditionReportSeeder> logger) : SeederBase(logger)
{
    // Used to pre-populate vehicle decode fields when the load is a Vehicle.
    private static readonly (string Vin, int Year, string Make, string Model, string BodyClass)[] SampleVehicles =
    [
        ("1HGCM82633A004352", 2020, "Honda", "Accord", "Sedan/Saloon"),
        ("5YJSA1E26MF123456", 2021, "Tesla", "Model S", "Hatchback"),
        ("WVWZZZ3CZWE123456", 2019, "Volkswagen", "Passat", "Sedan/Saloon"),
        ("1G1YY22G965123456", 2022, "Chevrolet", "Corvette", "Coupe"),
        ("2T1BURHE1JC123456", 2020, "Toyota", "Corolla", "Sedan/Saloon"),
        ("WBA3A5C51CF123456", 2021, "BMW", "328i", "Sedan/Saloon"),
        ("1FAHP3F29CL123456", 2019, "Ford", "Focus", "Hatchback"),
        ("JHMGE8H59DC123456", 2022, "Honda", "Fit", "Hatchback"),
        ("3N1AB7AP5KY123456", 2020, "Nissan", "Sentra", "Sedan/Saloon"),
        ("KMHD84LF1KU123456", 2021, "Hyundai", "Elantra", "Sedan/Saloon"),
        ("JN1TANT31U0123456", 2019, "Nissan", "Altima", "Sedan/Saloon"),
        ("5NPEB4AC1BH123456", 2022, "Hyundai", "Sonata", "Sedan/Saloon")
    ];

    private static readonly Dictionary<CargoInspectionPartCategory, string[]> DefectDescriptions = new()
    {
        // Vehicle parts
        [CargoInspectionPartCategory.VehicleFrontBumper]    = ["Scratch on front bumper", "Paint chip on front bumper", "Small crack in bumper cover"],
        [CargoInspectionPartCategory.VehicleRearBumper]     = ["Dent on rear bumper", "Scuff on rear bumper"],
        [CargoInspectionPartCategory.VehicleHood]           = ["Paint chip on hood", "Light scratch on hood"],
        [CargoInspectionPartCategory.VehicleRoof]           = ["Hairline scratch on roof", "Small dent on roof panel"],
        [CargoInspectionPartCategory.VehicleTrunkLiftgate]  = ["Scratch near trunk latch", "Dent on liftgate"],
        [CargoInspectionPartCategory.VehicleFrontLeftDoor]  = ["Door ding on front left", "Scratch on front left door"],
        [CargoInspectionPartCategory.VehicleFrontRightDoor] = ["Door ding on front right", "Scuff on front right door"],
        [CargoInspectionPartCategory.VehicleRearLeftDoor]   = ["Scratch on rear left door"],
        [CargoInspectionPartCategory.VehicleRearRightDoor]  = ["Scratch on rear right door"],
        [CargoInspectionPartCategory.VehicleFenders]        = ["Dent on quarter panel", "Scrape on fender"],
        [CargoInspectionPartCategory.VehicleWheels]         = ["Curb rash on alloy wheel", "Scuff on wheel rim"],
        [CargoInspectionPartCategory.VehicleMirrors]        = ["Mirror housing cracked", "Scuff on side mirror"],
        [CargoInspectionPartCategory.VehicleWindshield]     = ["Small chip in windshield", "Crack starting in lower windshield"],
        [CargoInspectionPartCategory.VehicleSideGlass]      = ["Scratch on side glass"],
        [CargoInspectionPartCategory.VehicleLights]         = ["Cracked headlight lens", "Foggy lens"],
        [CargoInspectionPartCategory.VehicleBodyPanels]     = ["Minor ding on body panel"],
        [CargoInspectionPartCategory.VehicleInterior]       = ["Stain on driver seat", "Scuff on dashboard"],

        // Container parts
        [CargoInspectionPartCategory.ContainerFrontWall]      = ["Bowed front wall", "Surface rust on front wall"],
        [CargoInspectionPartCategory.ContainerRearDoors]      = ["Bent right door", "Worn rear door gasket"],
        [CargoInspectionPartCategory.ContainerLeftWall]       = ["Pinhole in left wall", "Dent on left wall"],
        [CargoInspectionPartCategory.ContainerRightWall]      = ["Dent on right wall", "Surface rust on right wall"],
        [CargoInspectionPartCategory.ContainerRoof]           = ["Pinhole leak in roof", "Bowed roof panel"],
        [CargoInspectionPartCategory.ContainerFloor]          = ["Soft spot on plywood floor", "Stained floor"],
        [CargoInspectionPartCategory.ContainerLockingHardware]= ["Stiff locking rod", "Missing locking pin"],
        [CargoInspectionPartCategory.ContainerCornerCastings] = ["Worn corner casting", "Crack in corner casting"],
        [CargoInspectionPartCategory.ContainerSeal]           = ["Seal tampered", "Seal number does not match BoL"],

        // Generic / freight
        [CargoInspectionPartCategory.GenericWalls]      = ["Dent on trailer wall", "Hole in side wall"],
        [CargoInspectionPartCategory.GenericDoors]      = ["Sticky rear door", "Damaged door seal"],
        [CargoInspectionPartCategory.GenericFloor]      = ["Trailer floor soft spot", "Debris on trailer floor"],
        [CargoInspectionPartCategory.GenericRoof]       = ["Roof tear in fabric", "Roof bracing loose"],
        [CargoInspectionPartCategory.GenericLighting]   = ["Trailer marker light out", "Brake light intermittent"],
        [CargoInspectionPartCategory.GenericTires]      = ["Low tread on trailer tire", "Sidewall scuff"],
        [CargoInspectionPartCategory.GenericSecurement] = ["Strap frayed", "Missing securement chain", "E-track damaged"],

        [CargoInspectionPartCategory.Other] = ["Other minor issue documented", "See notes for details"]
    };

    public override string Name => nameof(ConditionReportSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 155;
    public override IReadOnlyList<string> DependsOn =>
        [nameof(LoadSeeder), nameof(EmployeeSeeder), nameof(ContainerSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<LoadConditionReport>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var loadRepository = context.TenantUnitOfWork.Repository<Load>();
        var reportRepository = context.TenantUnitOfWork.Repository<LoadConditionReport>();
        var employeeRepository = context.TenantUnitOfWork.Repository<Employee>();

        var loads = await loadRepository.GetListAsync(ct: cancellationToken);
        if (loads.Count == 0)
        {
            logger.LogWarning("No loads available for condition report seeding");
            LogCompleted(0);
            return;
        }

        var drivers = context.CreatedEmployees?.Drivers
            ?? await employeeRepository.GetListAsync(e => e.Role != null && e.Role.Name == TenantRoles.Driver, ct: cancellationToken);
        if (drivers.Count == 0)
        {
            logger.LogWarning("No drivers available for condition report seeding");
            LogCompleted(0);
            return;
        }

        var count = 0;

        // Create condition reports for about 40% of loads
        foreach (var load in loads.Take((int)(loads.Count * 0.4)))
        {
            var driver = random.Pick(drivers);
            var captureLocation = random.Pick((IList<RoutePoint>)(context.Region?.RoutePoints ?? []));

            var pickupReport = CreateConditionReport(
                load,
                driver,
                InspectionType.Pickup,
                captureLocation,
                load.PickedUpAt ?? load.DispatchedAt ?? DateTime.UtcNow.AddDays(-random.Next(1, 30)));
            await reportRepository.AddAsync(pickupReport, cancellationToken);
            count++;

            // 50% chance of a delivery inspection
            if (random.NextDouble() > 0.5)
            {
                var deliveryLocation = random.Pick((context.Region?.RoutePoints ?? []).Where(p => p != captureLocation).ToList());
                var deliveryReport = CreateConditionReport(
                    load,
                    driver,
                    InspectionType.Delivery,
                    deliveryLocation,
                    load.DeliveredAt ?? load.PickedUpAt?.AddHours(random.Next(4, 48)) ?? DateTime.UtcNow.AddDays(-random.Next(1, 15)));
                await reportRepository.AddAsync(deliveryReport, cancellationToken);
                count++;
            }
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private LoadConditionReport CreateConditionReport(
        Load load,
        Employee driver,
        InspectionType type,
        RoutePoint location,
        DateTime inspectedAt)
    {
        var report = LoadConditionReport.Create(
            loadId: load.Id,
            type: type,
            inspectedById: driver.Id,
            notes: null,
            latitude: location.Latitude + (random.NextDouble() - 0.5) * 0.01,
            longitude: location.Longitude + (random.NextDouble() - 0.5) * 0.01);

        report.InspectedAt = inspectedAt;
        report.InspectorSignature = GenerateFakeSignature();

        // Populate cargo-type-specific identifier fields
        if (load.Type == LoadType.Vehicle)
        {
            var (Vin, Year, Make, Model, BodyClass) = random.Pick(SampleVehicles);
            report.Vin = Vin;
            report.VehicleYear = Year;
            report.VehicleMake = Make;
            report.VehicleModel = Model;
            report.VehicleBodyClass = BodyClass;
        }
        else if (load.Type.IsContainerLoad())
        {
            // Reuse the container's existing ISO 6346 number + seal if attached
            report.ContainerNumber = load.Container?.Number ?? GenerateFallbackContainerNumber();
            report.SealNumber = load.Container?.SealNumber ?? $"SEAL{random.Next(100000, 999999)}";
        }

        // Pick 0-3 defects from the cargo-type-specific catalog
        var catalog = CargoInspectionPartCategoryExtensions.GetCatalogFor(load.Type);
        var defectCount = random.NextDouble() switch
        {
            < 0.6 => 0,
            < 0.9 => random.Next(1, 3),
            _ => random.Next(3, 5)
        };

        for (var i = 0; i < defectCount; i++)
        {
            var partCategory = random.Pick(catalog);
            var description = DefectDescriptions.TryGetValue(partCategory, out var descriptions)
                ? random.Pick(descriptions)
                : "Defect documented during inspection";

            report.Defects.Add(new ConditionDefect
            {
                PartCategory = partCategory,
                Description = description,
                Severity = PickSeverity()
            });
        }

        report.Notes = GenerateNotes(type, report.Defects.Count);
        return report;
    }

    private DefectSeverity PickSeverity()
    {
        return random.NextDouble() switch
        {
            < 0.6 => DefectSeverity.Minor,
            < 0.9 => DefectSeverity.Major,
            _ => DefectSeverity.OutOfService
        };
    }

    private static string GenerateNotes(InspectionType type, int defectCount)
    {
        if (defectCount == 0)
        {
            return type == InspectionType.Pickup
                ? "Cargo received in good condition. No pre-existing damage noted."
                : "Cargo delivered in same condition as received. No new damage.";
        }

        return type == InspectionType.Pickup
            ? $"Cargo has {defectCount} pre-existing issue(s) documented. Customer acknowledged condition."
            : $"Cargo delivered with {defectCount} noted condition issue(s). See defects for details.";
    }

    private string GenerateFallbackContainerNumber()
    {
        // ISO 6346: 4 letters (3 owner + U/J/Z category) + 7 digits
        var owners = new[] { "MSCU", "MAEU", "TCNU", "TGHU", "HLBU", "OOLU", "CMAU" };
        return $"{random.Pick(owners)}{random.Next(1000000, 9999999)}";
    }

    private string GenerateFakeSignature()
    {
        var signatureBytes = new byte[100];
        random.NextBytes(signatureBytes);
        return Convert.ToBase64String(signatureBytes);
    }
}

using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Utils;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Primitives.Enums.Safety;
using Logistics.Shared.Identity.Roles;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds sample DVIR (Driver Vehicle Inspection Report) records for testing.
/// </summary>
internal class DvirReportSeeder(ILogger<DvirReportSeeder> logger) : SeederBase(logger)
{
    private static readonly Dictionary<DvirInspectionCategory, string[]> defectDescriptions = new()
    {
        [DvirInspectionCategory.Tires] =
        [
            "Low tread depth on front driver tire",
            "Sidewall damage on rear passenger tire",
            "Uneven wear pattern on steer tires",
            "Small nail in rear drive tire"
        ],
        [DvirInspectionCategory.BrakesService] =
        [
            "Brake pads worn below minimum thickness",
            "Air leak detected in brake system",
            "Brake adjustment needed",
            "Squealing noise when braking"
        ],
        [DvirInspectionCategory.LightsHead] =
        [
            "Driver side headlight burned out",
            "Headlight lens cracked",
            "Misaligned headlight beam"
        ],
        [DvirInspectionCategory.LightsTail] =
        [
            "Tail light not working",
            "Tail light lens cracked",
            "Wiring issue with tail lights"
        ],
        [DvirInspectionCategory.LightsBrake] =
        [
            "Passenger side brake light out",
            "Intermittent brake light failure",
            "Brake light lens broken"
        ],
        [DvirInspectionCategory.Mirrors] =
        [
            "Cracked driver side mirror",
            "Mirror adjustment mechanism broken",
            "Convex mirror loose"
        ],
        [DvirInspectionCategory.Windshield] =
        [
            "Small crack in windshield",
            "Chip in windshield spreading",
            "Windshield seal leaking"
        ],
        [DvirInspectionCategory.Wheels] =
        [
            "Wiper blades streaking",
            "Wiper motor weak",
            "Washer fluid not spraying"
        ],
        [DvirInspectionCategory.FluidLevels] =
        [
            "Engine oil level low",
            "Coolant level below minimum",
            "Washer fluid empty",
            "Power steering fluid low"
        ],
        [DvirInspectionCategory.Horn] =
        [
            "Horn not working",
            "Horn sounds weak"
        ],
        [DvirInspectionCategory.CouplingDevices] =
        [
            "Fifth wheel latch not engaging properly",
            "Kingpin wear detected",
            "Glad hands leaking"
        ],
        [DvirInspectionCategory.Exhaust] =
        [
            "Exhaust leak detected",
            "Loose exhaust clamp",
            "Exhaust pipe dented"
        ]
    };

    private static readonly string[] driverNotes =
    [
        "All systems checked and operational.",
        "Vehicle in good working condition.",
        "Minor issues noted but vehicle safe to operate.",
        "Completed full inspection, no concerns.",
        "Pre-trip inspection complete.",
        "Post-trip inspection - no new issues found."
    ];

    private static readonly string[] mechanicNotes =
    [
        "Repairs completed, vehicle cleared for operation.",
        "Defects corrected as noted.",
        "All reported issues addressed.",
        "Parts ordered, vehicle in shop.",
        "Temporary repair made, permanent fix scheduled."
    ];

    public override string Name => nameof(DvirReportSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 158;
    public override IReadOnlyList<string> DependsOn =>
        [nameof(TruckSeeder), nameof(EmployeeSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<DvirReport>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var dvirRepository = context.TenantUnitOfWork.Repository<DvirReport>();
        var employeeRepository = context.TenantUnitOfWork.Repository<Employee>();
        var truckRepository = context.TenantUnitOfWork.Repository<Truck>();

        // Get drivers from context or load from database
        var drivers = context.CreatedEmployees?.Drivers
            ?? await employeeRepository.GetListAsync(e => e.Role != null && e.Role.Name == TenantRoles.Driver, ct: cancellationToken);

        if (drivers.Count == 0)
        {
            logger.LogWarning("No drivers available for DVIR seeding");
            LogCompleted(0);
            return;
        }

        // Get trucks
        var trucks = context.CreatedTrucks
            ?? await truckRepository.GetListAsync(ct: cancellationToken);

        if (trucks.Count == 0)
        {
            logger.LogWarning("No trucks available for DVIR seeding");
            LogCompleted(0);
            return;
        }

        var count = 0;

        // Create DVIRs for the past 30 days
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Each truck should have regular inspections
        foreach (var truck in trucks)
        {
            // Create 10-20 inspections per truck over the past month
            var inspectionCount = random.Next(10, 21);
            var currentDate = startDate;

            for (var i = 0; i < inspectionCount; i++)
            {
                var driver = random.Pick(drivers);
                var inspectionDate = currentDate.AddHours(random.Next(0, 48));

                if (inspectionDate > endDate) break;

                var location = random.Pick(RoutePoints.Points);
                var dvirType = random.NextDouble() < 0.6 ? DvirType.PreTrip : DvirType.PostTrip;

                var report = CreateDvirReport(driver, truck, location, inspectionDate, dvirType);
                await dvirRepository.AddAsync(report, cancellationToken);
                count++;

                currentDate = inspectionDate.AddDays(random.Next(1, 4));
            }
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private DvirReport CreateDvirReport(
        Employee driver,
        Truck truck,
        (Domain.Primitives.ValueObjects.Address Address, double Longitude, double Latitude) location,
        DateTime inspectionDate,
        DvirType dvirType)
    {
        var hasDefects = random.NextDouble() < 0.25; // 25% chance of defects
        var status = PickStatus(hasDefects);

        var report = new DvirReport
        {
            TruckId = truck.Id,
            DriverId = driver.Id,
            Type = dvirType,
            Status = status,
            InspectionDate = inspectionDate,
            Latitude = location.Latitude + (random.NextDouble() - 0.5) * 0.01,
            Longitude = location.Longitude + (random.NextDouble() - 0.5) * 0.01,
            OdometerReading = random.Next(50000, 500000),
            HasDefects = hasDefects,
            DriverSignature = GenerateFakeSignature(),
            DriverNotes = hasDefects ? "Issues noted during inspection." : random.Pick(driverNotes)
        };

        // Add defects if any
        if (hasDefects)
        {
            var defectCount = random.Next(1, 4);
            var categories = defectDescriptions.Keys.ToList();

            for (var i = 0; i < defectCount; i++)
            {
                var category = random.Pick(categories);
                var descriptions = defectDescriptions[category];
                var severity = PickDefectSeverity();

                var defect = new DvirDefect
                {
                    DvirReportId = report.Id,
                    Category = category,
                    Description = random.Pick(descriptions),
                    Severity = severity,
                    IsCorrected = status == DvirStatus.Cleared
                };

                if (defect.IsCorrected)
                {
                    defect.CorrectedAt = inspectionDate.AddHours(random.Next(2, 24));
                    defect.CorrectionNotes = "Defect repaired and verified.";
                }

                report.Defects.Add(defect);
            }
        }

        // Add review info for reviewed/cleared reports
        if (status is DvirStatus.Reviewed or DvirStatus.Cleared)
        {
            report.ReviewedAt = inspectionDate.AddHours(random.Next(1, 8));
            report.MechanicSignature = GenerateFakeSignature();
            report.MechanicNotes = random.Pick(mechanicNotes);
            report.DefectsCorrected = status == DvirStatus.Cleared || !hasDefects;
        }

        return report;
    }

    private DvirStatus PickStatus(bool hasDefects)
    {
        if (!hasDefects)
        {
            var roll = random.NextDouble();
            return roll switch
            {
                < 0.1 => DvirStatus.Draft,
                < 0.3 => DvirStatus.Submitted,
                _ => DvirStatus.Cleared
            };
        }

        var defectRoll = random.NextDouble();
        return defectRoll switch
        {
            < 0.1 => DvirStatus.Draft,
            < 0.25 => DvirStatus.Submitted,
            < 0.4 => DvirStatus.RequiresRepair,
            < 0.6 => DvirStatus.Reviewed,
            _ => DvirStatus.Cleared
        };
    }

    private DefectSeverity PickDefectSeverity()
    {
        var roll = random.NextDouble();
        return roll switch
        {
            < 0.6 => DefectSeverity.Minor,
            < 0.9 => DefectSeverity.Major,
            _ => DefectSeverity.OutOfService
        };
    }

    private string GenerateFakeSignature()
    {
        var signatureBytes = new byte[100];
        random.NextBytes(signatureBytes);
        return Convert.ToBase64String(signatureBytes);
    }
}

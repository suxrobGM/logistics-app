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
/// Seeds sample accident reports for testing.
/// </summary>
internal class AccidentReportSeeder(ILogger<AccidentReportSeeder> logger) : SeederBase(logger)
{
    private static readonly string[] weatherConditions =
    [
        "Clear", "Cloudy", "Rain", "Heavy Rain", "Snow", "Fog", "Sleet", "Windy"
    ];

    private static readonly string[] roadConditions =
    [
        "Dry", "Wet", "Icy", "Snowy", "Muddy", "Gravel", "Construction Zone"
    ];

    private static readonly string[] accidentDescriptions =
    [
        "Minor fender bender in parking lot while backing up.",
        "Rear-ended by another vehicle at red light. Other driver admitted fault.",
        "Sideswiped by merging vehicle on highway. Minor damage to door panel.",
        "Struck debris on highway causing tire blowout and minor damage.",
        "Other vehicle ran red light and struck passenger side.",
        "Lost traction on icy road and slid into guardrail.",
        "Backing into dock, misjudged distance and hit dock equipment.",
        "Cargo shifted during hard brake causing trailer damage.",
        "Side mirror clipped by passing vehicle in construction zone.",
        "Hit pothole at highway speed causing suspension damage."
    ];

    private static readonly string[] injuryDescriptions =
    [
        "Minor whiplash, driver went to urgent care for evaluation.",
        "Small cuts from broken glass, treated on scene.",
        "Back pain reported, driver taken to hospital for x-rays.",
        "No visible injuries but driver reported headache."
    ];

    private static readonly string[] damageDescriptions =
    [
        "Dent and scratches on rear bumper.",
        "Cracked taillight and minor body damage.",
        "Driver side mirror broken, door dented.",
        "Front bumper damage and cracked headlight.",
        "Trailer door mechanism damaged.",
        "Tire blown, rim scratched.",
        "Paint scraped on passenger side.",
        "Windshield cracked from debris impact."
    ];

    public override string Name => nameof(AccidentReportSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 160;
    public override IReadOnlyList<string> DependsOn =>
        [nameof(TruckSeeder), nameof(EmployeeSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<AccidentReport>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var accidentRepository = context.TenantUnitOfWork.Repository<AccidentReport>();
        var employeeRepository = context.TenantUnitOfWork.Repository<Employee>();
        var truckRepository = context.TenantUnitOfWork.Repository<Truck>();

        // Get drivers from context or load from database
        var drivers = context.CreatedEmployees?.Drivers
            ?? await employeeRepository.GetListAsync(e => e.Role != null && e.Role.Name == TenantRoles.Driver, ct: cancellationToken);

        if (drivers.Count == 0)
        {
            logger.LogWarning("No drivers available for accident report seeding");
            LogCompleted(0);
            return;
        }

        // Get trucks
        var trucks = context.CreatedTrucks
            ?? await truckRepository.GetListAsync(ct: cancellationToken);

        if (trucks.Count == 0)
        {
            logger.LogWarning("No trucks available for accident report seeding");
            LogCompleted(0);
            return;
        }

        var count = 0;
        var reportsToCreate = random.Next(5, 12);

        for (var i = 0; i < reportsToCreate; i++)
        {
            var driver = random.Pick(drivers);
            var truck = random.Pick(trucks);
            var location = random.Pick(RoutePoints.Points);
            var accidentDate = random.UtcDate(DateTime.UtcNow.AddMonths(-6), DateTime.UtcNow.AddDays(-1));

            var report = CreateAccidentReport(driver, truck, location, accidentDate);
            await accidentRepository.AddAsync(report, cancellationToken);
            count++;
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private AccidentReport CreateAccidentReport(
        Employee driver,
        Truck truck,
        (Domain.Primitives.ValueObjects.Address Address, double Longitude, double Latitude) location,
        DateTime accidentDate)
    {
        var hasInjuries = random.NextDouble() < 0.15; // 15% chance of injuries
        var hasDamage = random.NextDouble() < 0.85; // 85% chance of damage
        var hasPoliceReport = random.NextDouble() < 0.6; // 60% chance of police report
        var severity = PickSeverity(hasInjuries);
        var status = PickStatus();

        var report = new AccidentReport
        {
            DriverId = driver.Id,
            TruckId = truck.Id,
            Status = status,
            AccidentType = random.Pick(Enum.GetValues<AccidentType>()),
            Severity = severity,
            AccidentDateTime = accidentDate,
            Latitude = location.Latitude + (random.NextDouble() - 0.5) * 0.01,
            Longitude = location.Longitude + (random.NextDouble() - 0.5) * 0.01,
            Address = $"{location.Address.Line1}, {location.Address.City}, {location.Address.State} {location.Address.ZipCode}",
            Description = random.Pick(accidentDescriptions),
            WeatherConditions = random.Pick(weatherConditions),
            RoadConditions = random.Pick(roadConditions),
            AnyInjuries = hasInjuries,
            VehicleDamaged = hasDamage,
            VehicleDrivable = severity != AccidentSeverity.Severe && severity != AccidentSeverity.Fatal,
            PoliceReportFiled = hasPoliceReport
        };

        if (hasInjuries)
        {
            report.NumberOfInjuries = random.Next(1, 4);
            report.InjuryDescription = random.Pick(injuryDescriptions);
        }

        if (hasDamage)
        {
            report.VehicleDamageDescription = random.Pick(damageDescriptions);
            report.EstimatedDamageCost = random.Next(500, 15000);
        }

        if (hasPoliceReport)
        {
            report.PoliceReportNumber = $"PR-{random.Next(100000, 999999)}";
            report.PoliceDepartment = $"{location.Address.City} Police Department";
            report.PoliceOfficerName = $"Officer {GetRandomName()}";
            report.PoliceOfficerBadge = $"#{random.Next(1000, 9999)}";
        }

        // Add driver statement for submitted+ reports
        if (status != AccidentReportStatus.Draft)
        {
            report.DriverStatement = "I was operating the vehicle when the incident occurred. I followed all safety protocols and immediately reported the incident.";
            report.DriverSignature = GenerateFakeSignature();
            report.DriverSignedAt = accidentDate.AddHours(random.Next(1, 4));
        }

        // Add insurance info for some reports
        if (random.NextDouble() < 0.4 && status != AccidentReportStatus.Draft)
        {
            report.InsuranceNotified = true;
            report.InsuranceNotifiedAt = accidentDate.AddDays(random.Next(1, 3));
            report.InsuranceClaimNumber = $"CLM-{DateTime.UtcNow.Year}-{random.Next(100000, 999999)}";
        }

        return report;
    }

    private AccidentSeverity PickSeverity(bool hasInjuries)
    {
        if (hasInjuries)
        {
            return random.NextDouble() < 0.8 ? AccidentSeverity.Moderate : AccidentSeverity.Severe;
        }

        var roll = random.NextDouble();
        return roll switch
        {
            < 0.7 => AccidentSeverity.Minor,
            < 0.95 => AccidentSeverity.Moderate,
            _ => AccidentSeverity.Severe
        };
    }

    private AccidentReportStatus PickStatus()
    {
        var roll = random.NextDouble();
        return roll switch
        {
            < 0.15 => AccidentReportStatus.Draft,
            < 0.35 => AccidentReportStatus.Submitted,
            < 0.55 => AccidentReportStatus.UnderReview,
            < 0.75 => AccidentReportStatus.InsuranceFiled,
            _ => AccidentReportStatus.Resolved
        };
    }

    private string GetRandomName()
    {
        string[] firstNames = ["John", "Mike", "Sarah", "James", "David", "Maria", "Robert", "Lisa"];
        string[] lastNames = ["Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis"];
        return $"{random.Pick(firstNames)} {random.Pick(lastNames)}";
    }

    private string GenerateFakeSignature()
    {
        var signatureBytes = new byte[100];
        random.NextBytes(signatureBytes);
        return Convert.ToBase64String(signatureBytes);
    }
}

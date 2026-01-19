using System.Text.Json;
using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.DbMigrator.Utils;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds sample vehicle condition reports (DVIR - Driver Vehicle Inspection Reports).
/// </summary>
internal class ConditionReportSeeder(ILogger<ConditionReportSeeder> logger) : SeederBase(logger)
{
    private readonly Random _random = new();

    // Sample VIN data with decoded vehicle info
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

    private static readonly string[] DamageDescriptions =
    [
        "Minor scratch on door panel",
        "Small dent on rear bumper",
        "Paint chip on hood",
        "Scratch on front fender",
        "Scuff mark on side mirror",
        "Minor ding on quarter panel",
        "Light scratch on roof",
        "Small crack in taillight cover"
    ];

    private static readonly string[] DamageSeverities = ["minor", "moderate", "severe"];

    public override string Name => nameof(ConditionReportSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 155;
    public override IReadOnlyList<string> DependsOn =>
        [nameof(LoadSeeder), nameof(EmployeeSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<VehicleConditionReport>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var loadRepository = context.TenantUnitOfWork.Repository<Load>();
        var reportRepository = context.TenantUnitOfWork.Repository<VehicleConditionReport>();
        var employeeRepository = context.TenantUnitOfWork.Repository<Employee>();

        // Get loads (vehicle transport loads would be ideal, but we'll use any loads)
        var loads = await loadRepository.GetListAsync(ct: cancellationToken);
        if (loads.Count == 0)
        {
            Logger.LogWarning("No loads available for condition report seeding");
            LogCompleted(0);
            return;
        }

        // Get drivers from context or load from database
        var drivers = context.CreatedEmployees?.Drivers
            ?? await employeeRepository.GetListAsync(e => e.Role != null && e.Role.Name == "Driver", ct: cancellationToken);
        if (drivers.Count == 0)
        {
            Logger.LogWarning("No drivers available for condition report seeding");
            LogCompleted(0);
            return;
        }

        var count = 0;

        // Create condition reports for about 40% of loads
        foreach (var load in loads.Take((int)(loads.Count * 0.4)))
        {
            var driver = _random.Pick(drivers);
            var vehicle = _random.Pick(SampleVehicles);
            var captureLocation = _random.Pick(RoutePoints.Points);

            // Create pickup inspection
            var pickupReport = CreateConditionReport(
                load,
                driver,
                vehicle,
                InspectionType.Pickup,
                captureLocation,
                load.PickedUpAt ?? load.DispatchedAt ?? DateTime.UtcNow.AddDays(-_random.Next(1, 30)));
            await reportRepository.AddAsync(pickupReport, cancellationToken);
            count++;

            // Create delivery inspection (50% chance to have delivery inspection)
            if (_random.NextDouble() > 0.5)
            {
                var deliveryLocation = _random.Pick(RoutePoints.Points.Where(p => p != captureLocation).ToArray());
                var deliveryReport = CreateConditionReport(
                    load,
                    driver,
                    vehicle,
                    InspectionType.Delivery,
                    deliveryLocation,
                    load.DeliveredAt ?? load.PickedUpAt?.AddHours(_random.Next(4, 48)) ?? DateTime.UtcNow.AddDays(-_random.Next(1, 15)));
                await reportRepository.AddAsync(deliveryReport, cancellationToken);
                count++;
            }
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(count);
    }

    private VehicleConditionReport CreateConditionReport(
        Load load,
        Employee driver,
        (string Vin, int Year, string Make, string Model, string BodyClass) vehicle,
        InspectionType type,
        (Domain.Primitives.ValueObjects.Address Address, double Longitude, double Latitude) location,
        DateTime inspectedAt)
    {
        var damageMarkers = GenerateDamageMarkers();

        var report = VehicleConditionReport.Create(
            loadId: load.Id,
            vin: vehicle.Vin,
            type: type,
            inspectedById: driver.Id,
            damageMarkersJson: damageMarkers.Count > 0 ? JsonSerializer.Serialize(damageMarkers) : null,
            notes: GenerateNotes(type, damageMarkers.Count),
            latitude: location.Latitude + (_random.NextDouble() - 0.5) * 0.01,
            longitude: location.Longitude + (_random.NextDouble() - 0.5) * 0.01
        );

        // Set vehicle info from VIN decode
        report.VehicleYear = vehicle.Year;
        report.VehicleMake = vehicle.Make;
        report.VehicleModel = vehicle.Model;
        report.VehicleBodyClass = vehicle.BodyClass;
        report.InspectedAt = inspectedAt;
        report.InspectorSignature = GenerateFakeSignature();

        return report;
    }

    private List<object> GenerateDamageMarkers()
    {
        var markers = new List<object>();

        // 60% chance of no damage, 30% chance of 1-2 damages, 10% chance of 3+ damages
        var damageChance = _random.NextDouble();
        var damageCount = damageChance switch
        {
            < 0.6 => 0,
            < 0.9 => _random.Next(1, 3),
            _ => _random.Next(3, 6)
        };

        for (var i = 0; i < damageCount; i++)
        {
            markers.Add(new
            {
                x = Math.Round(_random.NextDouble(), 2),
                y = Math.Round(_random.NextDouble(), 2),
                description = _random.Pick(DamageDescriptions),
                severity = _random.Pick(DamageSeverities)
            });
        }

        return markers;
    }

    private string GenerateNotes(InspectionType type, int damageCount)
    {
        if (damageCount == 0)
        {
            return type == InspectionType.Pickup
                ? "Vehicle received in excellent condition. No pre-existing damage noted."
                : "Vehicle delivered in same condition as received. No new damage.";
        }

        return type == InspectionType.Pickup
            ? $"Vehicle has {damageCount} pre-existing damage point(s) documented. Customer acknowledged condition."
            : $"Vehicle delivered with {damageCount} noted condition issue(s). See damage markers for details.";
    }

    private string GenerateFakeSignature()
    {
        var signatureBytes = new byte[100];
        _random.NextBytes(signatureBytes);
        return Convert.ToBase64String(signatureBytes);
    }
}

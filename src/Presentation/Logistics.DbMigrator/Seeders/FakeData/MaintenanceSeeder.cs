using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Maintenance;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds sample maintenance service records for testing.
/// </summary>
internal class MaintenanceSeeder(ILogger<MaintenanceSeeder> logger) : SeederBase(logger)
{
    private static readonly string[] VendorNames =
    [
        "Fleet Services Inc",
        "Truck Pro",
        "National Tire & Battery",
        "Pep Boys Commercial",
        "Rush Truck Centers",
        "TA Truck Service",
        "Love's Truck Care",
        "Petro Stopping Centers",
        "Speedco",
        "Freightliner Dealer"
    ];

    private static readonly string[] WorkDescriptions = new Dictionary<MaintenanceType, string[]>
    {
        [MaintenanceType.OilChange] =
        [
            "Changed engine oil and filter. Used synthetic 15W-40.",
            "Full oil change service with filter replacement.",
            "Drained old oil, replaced filter, added 12 quarts synthetic."
        ],
        [MaintenanceType.TireRotation] =
        [
            "Rotated all tires, checked pressure and tread depth.",
            "Performed tire rotation front to rear.",
            "Rotated tires and balanced wheels."
        ],
        [MaintenanceType.BrakeInspection] =
        [
            "Inspected brake pads, rotors, and brake lines.",
            "Full brake system inspection - all within spec.",
            "Checked brake wear indicators and adjusted drums."
        ],
        [MaintenanceType.AnnualDotInspection] =
        [
            "Completed annual DOT inspection - passed all points.",
            "Full DOT compliance inspection performed.",
            "Annual safety inspection - vehicle certified."
        ],
        [MaintenanceType.PreventiveMaintenance] =
        [
            "PM service: oil, filters, fluids, inspection.",
            "Preventive maintenance service completed.",
            "Full PM service with multi-point inspection."
        ]
    }.SelectMany(kv => kv.Value.Select(v => v)).ToArray();

    private static readonly Dictionary<MaintenanceType, (decimal min, decimal max)> CostRanges = new()
    {
        [MaintenanceType.OilChange] = (150, 350),
        [MaintenanceType.TireRotation] = (50, 150),
        [MaintenanceType.TireReplacement] = (800, 2500),
        [MaintenanceType.BrakeInspection] = (100, 300),
        [MaintenanceType.BrakeReplacement] = (500, 1500),
        [MaintenanceType.AirFilterReplacement] = (50, 150),
        [MaintenanceType.FuelFilterReplacement] = (100, 250),
        [MaintenanceType.TransmissionService] = (300, 800),
        [MaintenanceType.CoolantFlush] = (150, 400),
        [MaintenanceType.BeltInspection] = (75, 200),
        [MaintenanceType.Battery] = (200, 500),
        [MaintenanceType.AnnualDotInspection] = (150, 400),
        [MaintenanceType.PreventiveMaintenance] = (300, 700),
        [MaintenanceType.EngineService] = (500, 2000),
        [MaintenanceType.SuspensionService] = (400, 1200),
        [MaintenanceType.ElectricalRepair] = (200, 800),
        [MaintenanceType.BodyWork] = (300, 1500),
        [MaintenanceType.HvacService] = (200, 600),
        [MaintenanceType.ExhaustSystem] = (300, 1000),
        [MaintenanceType.SteeringRepair] = (250, 900),
        [MaintenanceType.Other] = (100, 500)
    };

    private static readonly Dictionary<MaintenanceType, string[]> PartsUsed = new()
    {
        [MaintenanceType.OilChange] = ["Oil Filter", "Engine Oil 15W-40 (gallon)", "Drain Plug Gasket"],
        [MaintenanceType.TireReplacement] = ["Steer Tire 295/75R22.5", "Drive Tire 295/75R22.5", "Valve Stem"],
        [MaintenanceType.BrakeReplacement] = ["Brake Pad Set", "Brake Rotor", "Brake Hardware Kit"],
        [MaintenanceType.AirFilterReplacement] = ["Primary Air Filter", "Secondary Air Filter"],
        [MaintenanceType.Battery] = ["Heavy Duty Battery", "Battery Cable"],
        [MaintenanceType.FuelFilterReplacement] = ["Fuel Filter", "Fuel Line O-Ring"],
        [MaintenanceType.CoolantFlush] = ["Coolant (gallon)", "Thermostat", "Radiator Hose"],
        [MaintenanceType.TransmissionService] = ["Transmission Fluid", "Transmission Filter"]
    };

    public override string Name => nameof(MaintenanceSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 155; // Before DVIR (158)
    public override IReadOnlyList<string> DependsOn => [nameof(TruckSeeder)];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<MaintenanceRecord>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var recordRepository = context.TenantUnitOfWork.Repository<MaintenanceRecord>();
        var truckRepository = context.TenantUnitOfWork.Repository<Truck>();

        var trucks = context.CreatedTrucks
            ?? await truckRepository.GetListAsync(ct: cancellationToken);

        if (trucks.Count == 0)
        {
            Logger.LogWarning("No trucks available for maintenance seeding");
            LogCompleted(0);
            return;
        }

        var recordCount = 0;

        foreach (var truck in trucks)
        {
            // Create 5-15 service records per truck over the past 90 days
            var recordsToCreate = random.Next(5, 16);
            var startDate = DateTime.UtcNow.AddDays(-90);

            for (var i = 0; i < recordsToCreate; i++)
            {
                var serviceDate = random.UtcDate(startDate, DateTime.UtcNow);
                var maintenanceType = random.Pick(Enum.GetValues<MaintenanceType>());

                var record = CreateMaintenanceRecord(truck, maintenanceType, serviceDate);
                await recordRepository.AddAsync(record, cancellationToken);
                recordCount++;
            }
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        LogCompleted(recordCount);
    }

    private MaintenanceRecord CreateMaintenanceRecord(Truck truck, MaintenanceType maintenanceType, DateTime serviceDate)
    {
        var (minCost, maxCost) = CostRanges.GetValueOrDefault(maintenanceType, (100, 500));
        var totalCost = Math.Round((decimal)(random.NextDouble() * (double)(maxCost - minCost)) + minCost, 2);
        var laborRatio = 0.3 + random.NextDouble() * 0.4; // 30-70% labor
        var laborCost = Math.Round(totalCost * (decimal)laborRatio, 2);
        var partsCost = totalCost - laborCost;

        var record = new MaintenanceRecord
        {
            TruckId = truck.Id,
            MaintenanceType = maintenanceType,
            ServiceDate = serviceDate,
            OdometerReading = random.Next(100000, 500000),
            EngineHours = random.NextDouble() < 0.5 ? random.Next(5000, 20000) : null,
            VendorName = random.Pick(VendorNames),
            InvoiceNumber = $"INV-{random.Next(10000, 99999)}",
            LaborCost = laborCost,
            PartsCost = partsCost,
            TotalCost = totalCost,
            Description = maintenanceType.GetDescription(),
            WorkPerformed = random.Pick(WorkDescriptions)
        };

        // Add parts for some records (60% chance)
        if (random.NextDouble() < 0.6 && PartsUsed.TryGetValue(maintenanceType, out var parts))
        {
            var partsToAdd = random.Next(1, Math.Min(4, parts.Length + 1));
            var usedParts = new HashSet<string>();

            for (var i = 0; i < partsToAdd; i++)
            {
                var partName = random.Pick(parts);
                if (usedParts.Contains(partName)) continue;
                usedParts.Add(partName);

                var quantity = random.Next(1, 4);
                var unitCost = Math.Round((decimal)(10 + random.NextDouble() * 200), 2);

                record.Parts.Add(new MaintenancePart
                {
                    MaintenanceRecordId = record.Id,
                    PartName = partName,
                    PartNumber = $"PN-{random.Next(1000, 9999)}",
                    Quantity = quantity,
                    UnitCost = unitCost,
                    TotalCost = unitCost * quantity
                });
            }
        }

        return record;
    }
}

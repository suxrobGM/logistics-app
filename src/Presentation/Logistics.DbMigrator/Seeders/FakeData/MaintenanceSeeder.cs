using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Maintenance;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Enums.Maintenance;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds sample maintenance schedules and service records for testing.
/// </summary>
internal class MaintenanceSeeder(ILogger<MaintenanceSeeder> logger) : SeederBase(logger)
{
    private static readonly string[] vendorNames =
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

    private static readonly string[] workDescriptions = new Dictionary<MaintenanceType, string[]>
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

    private static readonly Dictionary<MaintenanceType, (decimal min, decimal max)> costRanges = new()
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

    private static readonly Dictionary<MaintenanceType, string[]> partsUsed = new()
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

    // Schedules should generally be based on these common maintenance types
    private static readonly MaintenanceType[] scheduledMaintenanceTypes =
    [
        MaintenanceType.OilChange,
        MaintenanceType.TireRotation,
        MaintenanceType.BrakeInspection,
        MaintenanceType.AirFilterReplacement,
        MaintenanceType.FuelFilterReplacement,
        MaintenanceType.AnnualDotInspection,
        MaintenanceType.PreventiveMaintenance,
        MaintenanceType.TransmissionService,
        MaintenanceType.CoolantFlush
    ];

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

        var scheduleRepository = context.TenantUnitOfWork.Repository<MaintenanceSchedule>();
        var recordRepository = context.TenantUnitOfWork.Repository<MaintenanceRecord>();
        var truckRepository = context.TenantUnitOfWork.Repository<Truck>();

        var trucks = context.CreatedTrucks
            ?? await truckRepository.GetListAsync(ct: cancellationToken);

        if (trucks.Count == 0)
        {
            logger.LogWarning("No trucks available for maintenance seeding");
            LogCompleted(0);
            return;
        }

        var scheduleCount = 0;
        var recordCount = 0;

        foreach (var truck in trucks)
        {
            // Create 3-5 maintenance schedules per truck
            var schedulesToCreate = random.Next(3, 6);
            var usedTypes = new HashSet<MaintenanceType>();

            for (var i = 0; i < schedulesToCreate; i++)
            {
                MaintenanceType maintenanceType;
                do
                {
                    maintenanceType = random.Pick(scheduledMaintenanceTypes);
                } while (usedTypes.Contains(maintenanceType) && usedTypes.Count < scheduledMaintenanceTypes.Length);

                if (usedTypes.Contains(maintenanceType)) continue;
                usedTypes.Add(maintenanceType);

                var schedule = CreateMaintenanceSchedule(truck, maintenanceType);
                await scheduleRepository.AddAsync(schedule, cancellationToken);
                scheduleCount++;
            }

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
        logger.LogInformation("Seeded {ScheduleCount} maintenance schedules and {RecordCount} service records", scheduleCount, recordCount);
        LogCompleted(recordCount);
    }

    private MaintenanceSchedule CreateMaintenanceSchedule(Truck truck, MaintenanceType maintenanceType)
    {
        var intervalType = PickIntervalType(maintenanceType);
        var lastServiceDate = DateTime.UtcNow.AddDays(-random.Next(10, 120));
        var lastServiceMileage = random.Next(100000, 400000);

        var schedule = new MaintenanceSchedule
        {
            TruckId = truck.Id,
            MaintenanceType = maintenanceType,
            IntervalType = intervalType,
            IsActive = true,
            LastServiceDate = lastServiceDate,
            LastServiceMileage = lastServiceMileage,
            Notes = $"Regular {maintenanceType} schedule"
        };

        // Set intervals and next due based on interval type
        switch (intervalType)
        {
            case MaintenanceIntervalType.Mileage:
                schedule.MileageInterval = GetMileageInterval(maintenanceType);
                schedule.NextDueMileage = lastServiceMileage + schedule.MileageInterval;
                break;

            case MaintenanceIntervalType.TimeBased:
                schedule.DaysInterval = GetDaysInterval(maintenanceType);
                schedule.NextDueDate = lastServiceDate.AddDays(schedule.DaysInterval.Value);
                break;

            case MaintenanceIntervalType.EngineHours:
                schedule.EngineHoursInterval = random.Next(200, 500);
                schedule.LastServiceEngineHours = random.Next(5000, 15000);
                schedule.NextDueEngineHours = schedule.LastServiceEngineHours + schedule.EngineHoursInterval;
                break;

            case MaintenanceIntervalType.Combined:
                schedule.MileageInterval = GetMileageInterval(maintenanceType);
                schedule.DaysInterval = GetDaysInterval(maintenanceType);
                schedule.NextDueMileage = lastServiceMileage + schedule.MileageInterval;
                schedule.NextDueDate = lastServiceDate.AddDays(schedule.DaysInterval.Value);
                break;
        }

        return schedule;
    }

    private MaintenanceRecord CreateMaintenanceRecord(Truck truck, MaintenanceType maintenanceType, DateTime serviceDate)
    {
        var (minCost, maxCost) = costRanges.GetValueOrDefault(maintenanceType, (100, 500));
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
            VendorName = random.Pick(vendorNames),
            InvoiceNumber = $"INV-{random.Next(10000, 99999)}",
            LaborCost = laborCost,
            PartsCost = partsCost,
            TotalCost = totalCost,
            Description = maintenanceType.GetDescription(),
            WorkPerformed = random.Pick(workDescriptions)
        };

        // Add parts for some records (60% chance)
        if (random.NextDouble() < 0.6 && partsUsed.TryGetValue(maintenanceType, out var parts))
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

    private MaintenanceIntervalType PickIntervalType(MaintenanceType maintenanceType)
    {
        // Some maintenance types have natural interval types
        return maintenanceType switch
        {
            MaintenanceType.OilChange => random.NextDouble() < 0.7
                ? MaintenanceIntervalType.Mileage
                : MaintenanceIntervalType.Combined,
            MaintenanceType.AnnualDotInspection => MaintenanceIntervalType.TimeBased,
            MaintenanceType.TireRotation => MaintenanceIntervalType.Mileage,
            _ => random.Pick(Enum.GetValues<MaintenanceIntervalType>())
        };
    }

    private int GetMileageInterval(MaintenanceType maintenanceType)
    {
        return maintenanceType switch
        {
            MaintenanceType.OilChange => random.Next(10000, 20000),
            MaintenanceType.TireRotation => random.Next(15000, 25000),
            MaintenanceType.BrakeInspection => random.Next(20000, 40000),
            MaintenanceType.AirFilterReplacement => random.Next(15000, 30000),
            MaintenanceType.FuelFilterReplacement => random.Next(20000, 40000),
            MaintenanceType.TransmissionService => random.Next(50000, 100000),
            MaintenanceType.CoolantFlush => random.Next(30000, 60000),
            _ => random.Next(15000, 50000)
        };
    }

    private int GetDaysInterval(MaintenanceType maintenanceType)
    {
        return maintenanceType switch
        {
            MaintenanceType.AnnualDotInspection => 365,
            MaintenanceType.OilChange => random.Next(60, 120),
            MaintenanceType.PreventiveMaintenance => random.Next(30, 90),
            _ => random.Next(90, 180)
        };
    }
}

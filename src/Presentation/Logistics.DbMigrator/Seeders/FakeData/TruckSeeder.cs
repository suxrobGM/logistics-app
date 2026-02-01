using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Models;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.DbMigrator.Seeders.FakeData;

/// <summary>
/// Seeds trucks assigned to drivers.
/// </summary>
internal class TruckSeeder(ILogger<TruckSeeder> logger) : SeederBase(logger)
{
    public override string Name => nameof(TruckSeeder);
    public override SeederType Type => SeederType.FakeData;
    public override int Order => 130;
    public override IReadOnlyList<string> DependsOn => [nameof(EmployeeSeeder)];

    private static readonly (string Make, string Model)[] freightTruckModels =
    [
        ("Freightliner", "Cascadia"),
        ("Peterbilt", "579"),
        ("Kenworth", "T680"),
        ("Volvo", "VNL 860"),
        ("International", "LT"),
        ("Mack", "Anthem"),
        ("Western Star", "5700XE"),
    ];

    private static readonly (string Make, string Model)[] carHaulerModels =
    [
        ("Peterbilt", "389"),
        ("Kenworth", "W900"),
        ("Freightliner", "Coronado"),
        ("Volvo", "VNL 760"),
        ("International", "HX"),
    ];

    private static readonly string[] usStates =
    [
        "TX", "CA", "FL", "GA", "IL", "OH", "PA", "NC", "MI", "NJ",
        "VA", "AZ", "TN", "IN", "MO", "WI", "CO", "AL", "SC", "LA"
    ];

    protected override async Task<bool> HasExistingDataAsync(SeederContext context, CancellationToken cancellationToken)
    {
        return await context.TenantUnitOfWork.Repository<Truck>().CountAsync(ct: cancellationToken) > 0;
    }

    public override async Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default)
    {
        LogStarting();

        var employees = context.CreatedEmployees ?? throw new InvalidOperationException("Employees not seeded");
        var drivers = employees.Drivers;

        if (drivers.Count == 0)
        {
            logger.LogWarning("No drivers available to assign trucks to");
            context.CreatedTrucks = [];
            LogCompleted(0);
            return;
        }

        var trucksList = new List<Truck>();
        var truckNumber = 101;
        var truckRepository = context.TenantUnitOfWork.Repository<Truck>();

        foreach (var driver in drivers)
        {
            var truckType = random.Pick([TruckType.CarHauler, TruckType.FreightTruck]);
            var truck = Truck.Create(truckNumber.ToString(), truckType, driver);
            truck.VehicleCapacity = truckType == TruckType.CarHauler ? 7 : 0;

            // Set make and model based on truck type
            var (make, model) = truckType == TruckType.CarHauler
                ? random.Pick(carHaulerModels)
                : random.Pick(freightTruckModels);
            truck.Make = make;
            truck.Model = model;

            // Set year (2018-2024)
            truck.Year = random.Next(2018, 2025);

            // Generate a realistic VIN (17 characters)
            truck.Vin = GenerateVin(make);

            // Generate license plate and state
            truck.LicensePlate = GenerateLicensePlate();
            truck.LicensePlateState = random.Pick(usStates);

            truckNumber++;
            trucksList.Add(truck);
            await truckRepository.AddAsync(truck, cancellationToken);
            logger.LogInformation("Created truck {Number} ({Year} {Make} {Model})", truck.Number, truck.Year, truck.Make, truck.Model);
        }

        await context.TenantUnitOfWork.SaveChangesAsync(cancellationToken);
        context.CreatedTrucks = trucksList;
        LogCompleted(trucksList.Count);
    }

    private string GenerateVin(string make)
    {
        // VIN format: WMI (3) + VDS (6) + VIS (8) = 17 characters
        // Using simplified realistic patterns based on manufacturer
        var wmi = make switch
        {
            "Freightliner" => "1FU",
            "Peterbilt" => "1XP",
            "Kenworth" => "1XK",
            "Volvo" => "4V4",
            "International" => "3HS",
            "Mack" => "1M1",
            "Western Star" => "5KK",
            _ => "1XX"
        };

        var vds = GenerateAlphanumeric(6);
        var vis = GenerateAlphanumeric(8);

        return $"{wmi}{vds}{vis}";
    }

    private string GenerateLicensePlate()
    {
        // Common US format: ABC-1234 or AB-12345
        var letters = GenerateLetters(3);
        var numbers = random.Next(1000, 9999);
        return $"{letters}-{numbers}";
    }

    private string GenerateAlphanumeric(int length)
    {
        const string chars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789"; // Excluding I, O, Q which aren't used in VINs
        return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }

    private string GenerateLetters(int length)
    {
        const string letters = "ABCDEFGHJKLMNPRSTUVWXYZ";
        return new string(Enumerable.Range(0, length).Select(_ => letters[random.Next(letters.Length)]).ToArray());
    }
}

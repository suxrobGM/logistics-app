using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Regions;

internal sealed class UsRegionProfile : IRegionProfile
{
    private readonly Random random = new();

    public Region Region => Region.Us;
    public string DisplayName => "US Demo";
    public CurrencyCode Currency => CurrencyCode.USD;
    public DistanceUnit DistanceUnit => DistanceUnit.Miles;
    public WeightUnit WeightUnit => WeightUnit.Pounds;
    public DateFormatType DateFormat => DateFormatType.US;
    public string Timezone => "America/New_York";

    public Address CompanyAddress => new()
    {
        Line1 = "7 Allstate Rd",
        City = "Dorchester",
        State = "MA",
        ZipCode = "02125",
        Country = "US"
    };

    public IReadOnlyList<RoutePoint> RoutePoints { get; } =
    [
        new(new Address { Line1 = "233 S Wacker Dr", City = "Chicago", State = "IL", ZipCode = "60606", Country = "US" }, -87.6298, 41.8781),
        new(new Address { Line1 = "1 Monument Cir", City = "Indianapolis", State = "IN", ZipCode = "46204", Country = "US" }, -86.1581, 39.7684),
        new(new Address { Line1 = "100 N Capitol Ave", City = "Lansing", State = "MI", ZipCode = "48933", Country = "US" }, -84.5555, 42.7325),
        new(new Address { Line1 = "600 Woodward Ave", City = "Detroit", State = "MI", ZipCode = "48226", Country = "US" }, -83.0458, 42.3314),
        new(new Address { Line1 = "151 W Jefferson", City = "Louisville", State = "KY", ZipCode = "40202", Country = "US" }, -85.7585, 38.2527),
        new(new Address { Line1 = "600 Commerce St", City = "Nashville", State = "TN", ZipCode = "37203", Country = "US" }, -86.7816, 36.1627),
        new(new Address { Line1 = "1100 Congress Ave", City = "Austin", State = "TX", ZipCode = "78701", Country = "US" }, -97.7431, 30.2672),
        new(new Address { Line1 = "55 Trinity Ave SW", City = "Atlanta", State = "GA", ZipCode = "30303", Country = "US" }, -84.3902, 33.7490),
        new(new Address { Line1 = "1500 Marilla St", City = "Dallas", State = "TX", ZipCode = "75201", Country = "US" }, -96.7970, 32.7767),
        new(new Address { Line1 = "1411 Market St", City = "St. Louis", State = "MO", ZipCode = "63103", Country = "US" }, -90.1994, 38.6270)
    ];

    public IReadOnlyList<TerminalSeed> Terminals { get; } =
    [
        new("Port of Los Angeles", "USLAX", "US", TerminalType.SeaPort,
            new Address { Line1 = "425 S Palos Verdes St", City = "San Pedro", State = "CA", ZipCode = "90731", Country = "US" }),
        new("Port of Long Beach", "USLGB", "US", TerminalType.SeaPort,
            new Address { Line1 = "415 W Ocean Blvd", City = "Long Beach", State = "CA", ZipCode = "90802", Country = "US" }),
        new("Port of New York / New Jersey", "USNYC", "US", TerminalType.SeaPort,
            new Address { Line1 = "4 World Trade Center", City = "New York", State = "NY", ZipCode = "10007", Country = "US" }),
        new("Port of Houston", "USHOU", "US", TerminalType.SeaPort,
            new Address { Line1 = "111 East Loop N", City = "Houston", State = "TX", ZipCode = "77029", Country = "US" }),
        new("Port of Savannah", "USSAV", "US", TerminalType.SeaPort,
            new Address { Line1 = "2 E Bay St", City = "Savannah", State = "GA", ZipCode = "31401", Country = "US" }),
        new("BNSF Chicago Logistics Park", "USCHI", "US", TerminalType.RailTerminal,
            new Address { Line1 = "26664 S Baseline Rd", City = "Elwood", State = "IL", ZipCode = "60421", Country = "US" }),
        new("Memphis Inland Depot", "USMEM", "US", TerminalType.InlandDepot,
            new Address { Line1 = "3030 Airways Blvd", City = "Memphis", State = "TN", ZipCode = "38131", Country = "US" })
    ];

    public IReadOnlyList<TruckMakeModel> FreightTruckModels { get; } =
    [
        new("Freightliner", "Cascadia", "1FU"),
        new("Peterbilt", "579", "1XP"),
        new("Kenworth", "T680", "1XK"),
        new("Volvo", "VNL 860", "4V4"),
        new("International", "LT", "3HS"),
        new("Mack", "Anthem", "1M1"),
        new("Western Star", "5700XE", "5KK")
    ];

    public IReadOnlyList<TruckMakeModel> CarHaulerModels { get; } =
    [
        new("Peterbilt", "389", "1XP"),
        new("Kenworth", "W900", "1XK"),
        new("Freightliner", "Coronado", "1FU"),
        new("Volvo", "VNL 760", "4V4"),
        new("International", "HX", "3HS")
    ];

    public IReadOnlyList<TruckMakeModel> ContainerTruckModels { get; } =
    [
        new("International", "HX", "3HS"),
        new("Kenworth", "T880", "1XK"),
        new("Peterbilt", "567", "1XP")
    ];

    public IReadOnlyList<string> TripCorridorNames { get; } =
    [
        "Midwest Sweep", "Southeast Run", "Texas Triangle", "Great Lakes Corridor",
        "I-40 Eastbound", "I-95 Northbound", "Coast-to-Coast", "Heartland Express",
        "Pacific Northwest Loop", "Rust Belt Route"
    ];

    public IReadOnlyList<string> ReeferCargoTypes { get; } =
    [
        "Frozen Meat", "Dairy Products", "Fresh Produce", "Pharmaceuticals",
        "Ice Cream", "Frozen Seafood", "Yogurt", "Frozen Pizza"
    ];

    public IReadOnlyList<string> HazMatCargoTypes { get; } =
    [
        "Industrial Solvent", "Lithium Batteries", "Compressed Gas",
        "Paint & Coatings", "Fertilizer", "Cleaning Chemicals"
    ];

    public IReadOnlyList<string> TankCargoTypes { get; } =
    [
        "Vegetable Oil", "Industrial Chemicals", "Liquid Sugar", "Crude Oil", "Diesel Fuel"
    ];

    public IReadOnlyList<VehicleMakeModel> CarHaulerCargoVehicles { get; } =
    [
        new("Toyota", "Camry"),
        new("Honda", "Civic"),
        new("Ford", "F-150"),
        new("Chevrolet", "Silverado"),
        new("Tesla", "Model 3"),
        new("Ram", "1500"),
        new("Honda", "CR-V"),
        new("Nissan", "Altima"),
        new("Jeep", "Wrangler"),
        new("BMW", "X5"),
        new("Subaru", "Outback"),
        new("Hyundai", "Sonata")
    ];

    public IReadOnlyList<string> ContainerOwnerCodes { get; } = ["MSCU", "APLU", "HLXU", "TCLU", "CMAU"];

    private static readonly string[] UsStates =
    [
        "TX", "CA", "FL", "GA", "IL", "OH", "PA", "NC", "MI", "NJ",
        "VA", "AZ", "TN", "IN", "MO", "WI", "CO", "AL", "SC", "LA"
    ];

    public string GenerateVin(string make)
    {
        var wmi = FreightTruckModels.Concat(CarHaulerModels).Concat(ContainerTruckModels)
            .FirstOrDefault(m => m.Make == make)?.VinWmi ?? "1XX";
        return wmi + GenerateAlphanumeric(14);
    }

    public LicensePlate GeneratePlate()
    {
        var letters = GenerateLetters(3);
        var numbers = random.Next(1000, 9999);
        var state = UsStates[random.Next(UsStates.Length)];
        return new LicensePlate($"{letters}-{numbers}", state);
    }

    private string GenerateAlphanumeric(int length)
    {
        const string chars = "ABCDEFGHJKLMNPRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, length).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }

    private string GenerateLetters(int length)
    {
        const string letters = "ABCDEFGHJKLMNPRSTUVWXYZ";
        return new string(Enumerable.Range(0, length).Select(_ => letters[random.Next(letters.Length)]).ToArray());
    }
}

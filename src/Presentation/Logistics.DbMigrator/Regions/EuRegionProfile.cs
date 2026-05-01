using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Regions;

internal sealed class EuRegionProfile : IRegionProfile
{
    private readonly Random random = new();

    public Region Region => Region.Eu;
    public string DisplayName => "EU Demo";
    public CurrencyCode Currency => CurrencyCode.EUR;
    public DistanceUnit DistanceUnit => DistanceUnit.Kilometers;
    public WeightUnit WeightUnit => WeightUnit.Kilograms;
    public DateFormatType DateFormat => DateFormatType.European;
    public string Timezone => "Europe/Berlin";

    public Address CompanyAddress => new()
    {
        Line1 = "Mainzer Landstraße 36",
        City = "Frankfurt am Main",
        State = "Hesse",
        ZipCode = "60325",
        Country = "DE"
    };

    public IReadOnlyList<RoutePoint> RoutePoints { get; } =
    [
        new(new Address { Line1 = "Wilhelminakade 909", City = "Rotterdam", State = "South Holland", ZipCode = "3072 AP", Country = "NL" }, 4.4818, 51.9067),
        new(new Address { Line1 = "Brouwersvliet 33", City = "Antwerp", State = "Antwerp", ZipCode = "2000", Country = "BE" }, 4.4025, 51.2194),
        new(new Address { Line1 = "Bei St. Annen 1", City = "Hamburg", State = "Hamburg", ZipCode = "20457", Country = "DE" }, 9.9937, 53.5511),
        new(new Address { Line1 = "Schifferstraße 24", City = "Duisburg", State = "North Rhine-Westphalia", ZipCode = "47059", Country = "DE" }, 6.7623, 51.4344),
        new(new Address { Line1 = "1 Pl. Bellecour", City = "Lyon", State = "Auvergne-Rhône-Alpes", ZipCode = "69002", Country = "FR" }, 4.8357, 45.7640),
        new(new Address { Line1 = "Via Manzoni 12", City = "Milano", State = "Lombardy", ZipCode = "20121", Country = "IT" }, 9.1900, 45.4642),
        new(new Address { Line1 = "Carrer de Mallorca 401", City = "Barcelona", State = "Catalonia", ZipCode = "08013", Country = "ES" }, 2.1734, 41.3851),
        new(new Address { Line1 = "Plac Defilad 1", City = "Warszawa", State = "Masovian", ZipCode = "00-901", Country = "PL" }, 21.0118, 52.2297),
        new(new Address { Line1 = "Marienplatz 1", City = "München", State = "Bavaria", ZipCode = "80331", Country = "DE" }, 11.5755, 48.1374),
        new(new Address { Line1 = "Place Charles de Gaulle", City = "Paris", State = "Île-de-France", ZipCode = "75008", Country = "FR" }, 2.3522, 48.8566)
    ];

    public IReadOnlyList<TerminalSeed> Terminals { get; } =
    [
        new("Port of Antwerp-Bruges", "BEANR", "BE", TerminalType.SeaPort,
            new Address { Line1 = "Zaha Hadidplein 1", City = "Antwerp", State = "Antwerp", ZipCode = "2030", Country = "BE" }),
        new("Port of Rotterdam", "NLRTM", "NL", TerminalType.SeaPort,
            new Address { Line1 = "Wilhelminakade 909", City = "Rotterdam", State = "South Holland", ZipCode = "3072 AP", Country = "NL" }),
        new("Port of Hamburg", "DEHAM", "DE", TerminalType.SeaPort,
            new Address { Line1 = "Bei St. Annen 1", City = "Hamburg", State = "Hamburg", ZipCode = "20457", Country = "DE" }),
        new("Port of Bremerhaven", "DEBRV", "DE", TerminalType.SeaPort,
            new Address { Line1 = "Senator-Borttscheller-Straße 1", City = "Bremerhaven", State = "Bremen", ZipCode = "27568", Country = "DE" }),
        new("Duisburger Hafen Intermodal", "DEDUI", "DE", TerminalType.RailTerminal,
            new Address { Line1 = "Alte Ruhrorter Str. 42-52", City = "Duisburg", State = "North Rhine-Westphalia", ZipCode = "47119", Country = "DE" }),
        new("Lyon Inland Depot", "FRLYS", "FR", TerminalType.InlandDepot,
            new Address { Line1 = "Rue de la Tour", City = "Lyon", State = "Auvergne-Rhône-Alpes", ZipCode = "69007", Country = "FR" }),
        new("Port of Le Havre", "FRLEH", "FR", TerminalType.SeaPort,
            new Address { Line1 = "Terre-Plein de la Barre", City = "Le Havre", State = "Normandy", ZipCode = "76600", Country = "FR" })
    ];

    public IReadOnlyList<TruckMakeModel> FreightTruckModels { get; } =
    [
        new("Scania", "R 500", "YS2"),
        new("MAN", "TGX 18.510", "WMA"),
        new("DAF", "XF 480", "XLR"),
        new("Iveco", "S-Way", "ZCF"),
        new("Volvo", "FH16", "YV2"),
        new("Mercedes-Benz", "Actros 1851", "WDB"),
        new("Renault", "T High", "VF6")
    ];

    public IReadOnlyList<TruckMakeModel> CarHaulerModels { get; } =
    [
        new("Scania", "R 450", "YS2"),
        new("MAN", "TGS 26.440", "WMA"),
        new("Volvo", "FH 460", "YV2"),
        new("Mercedes-Benz", "Actros 2545", "WDB"),
        new("DAF", "XF 450 FAN", "XLR")
    ];

    public IReadOnlyList<TruckMakeModel> ContainerTruckModels { get; } =
    [
        new("Scania", "G 410", "YS2"),
        new("MAN", "TGX 18.470 4x2", "WMA"),
        new("Volvo", "FH 500", "YV2"),
        new("DAF", "XF 480 FT", "XLR")
    ];

    public IReadOnlyList<string> TripCorridorNames { get; } =
    [
        "Rhine Corridor", "Mediterranean Run", "Benelux Loop", "Iberian Sweep",
        "Alpine Crossing", "Baltic Route", "Hanseatic Express", "Po Valley Run",
        "Donau Eastbound", "Channel Crossing"
    ];

    public IReadOnlyList<string> ReeferCargoTypes { get; } =
    [
        "Dairy Products", "Frozen Meat", "Fresh Produce", "Pharmaceuticals",
        "Fresh Seafood", "Cheese & Yogurt", "Frozen Bakery", "Wine"
    ];

    public IReadOnlyList<string> HazMatCargoTypes { get; } =
    [
        "ADR Class 3 Flammables", "Compressed Gas", "Lithium Batteries",
        "Industrial Chemicals", "Fertilizer", "Paint & Coatings"
    ];

    public IReadOnlyList<string> TankCargoTypes { get; } =
    [
        "Olive Oil", "Industrial Chemicals", "Bulk Wine", "Liquid Sugar", "Diesel Fuel"
    ];

    public IReadOnlyList<VehicleMakeModel> CarHaulerCargoVehicles { get; } =
    [
        new("Volkswagen", "Golf"),
        new("Renault", "Clio"),
        new("Peugeot", "308"),
        new("Škoda", "Octavia"),
        new("BMW", "3 Series"),
        new("Mercedes-Benz", "C-Class"),
        new("Fiat", "500"),
        new("Audi", "A4"),
        new("Opel", "Astra"),
        new("Toyota", "Yaris"),
        new("SEAT", "Leon"),
        new("Volvo", "XC60")
    ];

    public IReadOnlyList<string> ContainerOwnerCodes { get; } = ["MAEU", "CMAU", "HLBU", "MSCU", "MSKU"];

    // Country code → weight (more weight = more frequent in plate generation).
    // Roughly mirrors EU truck-registration share: DE/PL/NL/IT/FR/ES/BE largest.
    private static readonly (string Country, int Weight)[] PlateCountries =
    [
        ("DE", 25), ("PL", 18), ("NL", 12), ("IT", 10),
        ("FR", 10), ("ES", 8), ("BE", 7), ("AT", 5), ("CZ", 5)
    ];

    private static readonly int TotalPlateWeight = PlateCountries.Sum(p => p.Weight);

    public string GenerateVin(string make)
    {
        var wmi = FreightTruckModels.Concat(CarHaulerModels).Concat(ContainerTruckModels)
            .FirstOrDefault(m => m.Make == make)?.VinWmi ?? "WXX";
        return wmi + GenerateAlphanumeric(14);
    }

    public LicensePlate GeneratePlate()
    {
        var country = PickWeightedCountry();
        var number = country switch
        {
            "DE" => $"{GenerateLetters(random.Next(1, 4))}-{GenerateLetters(random.Next(1, 3))} {random.Next(1, 9999)}",
            "NL" => $"{random.Next(10, 100):D2}-{GenerateLetters(3)}-{random.Next(0, 10)}",
            "FR" => $"{GenerateLetters(2)}-{random.Next(100, 1000):D3}-{GenerateLetters(2)}",
            "BE" => $"{random.Next(1, 10)}-{GenerateLetters(3)}-{random.Next(100, 1000):D3}",
            "IT" => $"{GenerateLetters(2)}{random.Next(100, 1000):D3}{GenerateLetters(2)}",
            "ES" => $"{random.Next(1000, 10000):D4} {GenerateLetters(3)}",
            "PL" => $"{GenerateLetters(2)} {GenerateAlphanumeric(5)}",
            "AT" => $"W-{GenerateAlphanumeric(5)}",
            "CZ" => $"{random.Next(1, 10)}{GenerateLetters(2)} {random.Next(1000, 10000):D4}",
            _ => $"{GenerateLetters(3)}-{random.Next(1000, 9999)}"
        };
        return new LicensePlate(number, country);
    }

    private string PickWeightedCountry()
    {
        var roll = random.Next(TotalPlateWeight);
        var cumulative = 0;
        foreach (var (country, weight) in PlateCountries)
        {
            cumulative += weight;
            if (roll < cumulative)
            {
                return country;
            }
        }
        return PlateCountries[0].Country;
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

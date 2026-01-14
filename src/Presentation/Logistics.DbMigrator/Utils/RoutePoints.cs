using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.DbMigrator.Utils;

/// <summary>
/// Shared route points for generating fake load and trip data.
/// </summary>
internal static class RoutePoints
{
    public static readonly (Address Address, double Longitude, double Latitude)[] Points =
    [
        (new Address { Line1 = "233 S Wacker Dr", City = "Chicago", State = "IL", ZipCode = "60606", Country = "USA" },
            -87.6298, 41.8781),
        (new Address { Line1 = "1 Monument Cir", City = "Indianapolis", State = "IN", ZipCode = "46204", Country = "USA" },
            -86.1581, 39.7684),
        (new Address { Line1 = "100 N Capitol Ave", City = "Lansing", State = "MI", ZipCode = "48933", Country = "USA" },
            -84.5555, 42.7325),
        (new Address { Line1 = "600 Woodward Ave", City = "Detroit", State = "MI", ZipCode = "48226", Country = "USA" },
            -83.0458, 42.3314),
        (new Address { Line1 = "151 W Jefferson", City = "Louisville", State = "KY", ZipCode = "40202", Country = "USA" },
            -85.7585, 38.2527),
        (new Address { Line1 = "600 Commerce St", City = "Nashville", State = "TN", ZipCode = "37203", Country = "USA" },
            -86.7816, 36.1627),
        (new Address { Line1 = "1100 Congress Ave", City = "Austin", State = "TX", ZipCode = "78701", Country = "USA" },
            -97.7431, 30.2672)
    ];
}

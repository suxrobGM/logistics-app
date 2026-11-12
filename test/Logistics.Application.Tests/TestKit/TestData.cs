using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Tests.TestKit;

/// <summary>
/// Valid entities with only the fields tests vary as parameters; set anything else on the result.
/// Also compiled into the AI tests through a linked file.
/// </summary>
internal static class TestData
{
    public static Address Address(string country = "US") => new()
    {
        Line1 = "1 Main St",
        City = "Dallas",
        State = "TX",
        ZipCode = "75201",
        Country = country
    };

    /// <summary>A null <paramref name="customer"/> leaves the navigation unset, as most handler tests never read it.</summary>
    public static Load Load(
        LoadType type = LoadType.GeneralFreight,
        decimal deliveryCost = 1000m,
        Customer? customer = null,
        string originCountry = "US",
        string destinationCountry = "US") => new()
    {
        Name = "Test Load",
        Type = type,
        Customer = customer!,
        CustomerId = customer?.Id ?? Guid.Empty,
        OriginAddress = Address(originCountry),
        OriginLocation = new GeoPoint(0, 0),
        DestinationAddress = Address(destinationCountry),
        DestinationLocation = new GeoPoint(0, 0),
        DeliveryCost = new Money { Amount = deliveryCost, Currency = "USD" }
    };

    public static Truck Truck(Employee? driver = null, TruckType type = TruckType.FreightTruck, string number = "T-1") => new()
    {
        Number = number,
        Type = type,
        MainDriver = driver,
        MainDriverId = driver?.Id
    };

    public static Employee Driver(Guid? id = null, params DriverLicense[] licenses) => new()
    {
        Id = id ?? Guid.NewGuid(),
        Email = "driver@test.com",
        FirstName = "John",
        LastName = "Doe",
        Salary = Money.Zero("USD"),
        Licenses = [.. licenses]
    };
}

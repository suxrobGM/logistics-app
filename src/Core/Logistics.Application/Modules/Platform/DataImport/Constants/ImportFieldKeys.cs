namespace Logistics.Application.Modules.Platform.DataImport.Constants;

/// <summary>
///     Field keys shared by the catalogue, the presets and the importers. They are also the wire keys
///     the client maps columns to.
/// </summary>
internal static class ImportFieldKeys
{
    public const string Name = "name";
    public const string Email = "email";
    public const string Phone = "phone";
    public const string Notes = "notes";
    public const string TaxId = "taxId";
    public const string McNumber = "mcNumber";

    public const string FirstName = "firstName";
    public const string LastName = "lastName";
    public const string FullName = "fullName";
    public const string HireDate = "hireDate";

    public const string Number = "number";
    public const string Type = "type";
    public const string Status = "status";
    public const string Vin = "vin";
    public const string Make = "make";
    public const string Model = "model";
    public const string Year = "year";
    public const string LicensePlate = "licensePlate";
    public const string LicensePlateState = "licensePlateState";
    public const string Driver = "driver";
    public const string VehicleCapacity = "vehicleCapacity";
    public const string Length = "length";

    public const string Reference = "reference";
    public const string Customer = "customer";
    public const string Rate = "rate";
    public const string Truck = "truck";
    public const string PickupDate = "pickupDate";
    public const string DeliveryDate = "deliveryDate";

    public const string Street = "street";
    public const string Street2 = "street2";
    public const string City = "city";
    public const string State = "state";
    public const string Zip = "zip";
    public const string Country = "country";
    public const string FullAddress = "fullAddress";

    public const string PickupPrefix = "pickup";
    public const string DeliveryPrefix = "delivery";

    /// <summary>
    ///     The key of an address part under a prefix: <c>Address("pickup", City)</c> is <c>pickupCity</c>.
    /// </summary>
    public static string Address(string? prefix, string part) =>
        string.IsNullOrEmpty(prefix) ? part : prefix + char.ToUpperInvariant(part[0]) + part[1..];
}

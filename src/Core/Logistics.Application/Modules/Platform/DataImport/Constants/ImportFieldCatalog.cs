using Logistics.Domain.Primitives.Enums;
using static Logistics.Application.Modules.Platform.DataImport.Constants.ImportFieldKeys;

namespace Logistics.Application.Modules.Platform.DataImport.Constants;

/// <summary>
///     The fields each import accepts and the header synonyms that auto-map to them. Add a spelling
///     here when a carrier's export uses one we miss; add a source-specific override to
///     <see cref="ImportPresets"/> only when a header means different things in different systems.
/// </summary>
internal static class ImportFieldCatalog
{
    private static readonly string[] PickupPrefixes = ["Pickup", "Pick Up", "PU", "Origin", "Shipper", "From"];
    private static readonly string[] DeliveryPrefixes = ["Delivery", "Drop", "Drop Off", "Del", "Destination", "Consignee", "To"];

    private static readonly IReadOnlyList<ImportField> Customers =
    [
        Field(Name, "Customer name", true, "Customer Name", "Customer", "Company", "Company Name", "Name", "Broker",
            "Broker Name", "Bill To", "Legal Name"),
        Field(Email, "Email", false, "Email", "E-mail", "Email Address", "Contact Email", "Billing Email"),
        Field(Phone, "Phone", false, "Phone", "Phone Number", "Telephone", "Tel", "Contact Phone", "Office Phone"),
        Field(TaxId, "Tax ID / VAT", false, "Tax ID", "EIN", "TIN", "VAT", "VAT Number", "Tax Number"),
        Field(McNumber, "MC number (saved to notes)", false, "MC", "MC #", "MC Number", "MC No", "Docket Number"),
        Field(Notes, "Notes", false, "Notes", "Note", "Comments", "Memo"),
        .. AddressFields(null, "")
    ];

    private static readonly IReadOnlyList<ImportField> Drivers =
    [
        Field(FirstName, "First name", false, "First Name", "First", "Given Name", "FirstName"),
        Field(LastName, "Last name", false, "Last Name", "Last", "Surname", "Family Name", "LastName"),
        Field(FullName, "Full name (split into first and last)", false, "Name", "Full Name", "Driver", "Driver Name"),
        Field(Email, "Email", true, "Email", "E-mail", "Email Address", "Driver Email"),
        Field(Phone, "Phone", false, "Phone", "Phone Number", "Cell", "Cell Phone", "Mobile", "Mobile Phone",
            "Contact Number"),
        Field(HireDate, "Hire date", false, "Hire Date", "Hired", "Date Hired", "Start Date", "Hired On", "Joined"),
        .. AddressFields(null, "")
    ];

    private static readonly IReadOnlyList<ImportField> Trucks =
    [
        Field(Number, "Truck number", true, "Unit", "Unit #", "Unit Number", "Unit No", "Truck", "Truck #",
            "Truck Number", "Truck No", "Truck ID", "Number", "Tractor", "Tractor #", "Power Unit"),
        Field(ImportFieldKeys.Type, "Truck type", false, "Truck Type", "Type", "Unit Type", "Vehicle Type"),
        Field(Vin, "VIN", false, "VIN", "VIN #", "VIN Number", "Serial Number"),
        Field(Make, "Make", false, "Make", "Manufacturer"),
        Field(Model, "Model", false, "Model"),
        Field(Year, "Year", false, "Year", "Model Year", "Yr"),
        Field(LicensePlate, "License plate", false, "Plate", "Plate #", "Plate Number", "License Plate", "License Plate #",
            "Tag", "Tag #", "Registration"),
        Field(LicensePlateState, "Plate state / region", false, "Plate State", "License State", "License Plate State",
            "Registration State", "Tag State", "State"),
        Field(Driver, "Driver (name or email)", false, "Driver", "Driver Name", "Main Driver", "Driver Email",
            "Assigned Driver"),
        Field(VehicleCapacity, "Vehicle capacity (car haulers)", false, "Capacity", "Vehicle Capacity", "Car Capacity"),
        Field(Status, "Status", false, "Status", "Truck Status")
    ];

    private static readonly IReadOnlyList<ImportField> Trailers =
    [
        Field(Number, "Trailer number", true, "Unit", "Unit #", "Unit Number", "Unit No", "Trailer", "Trailer #",
            "Trailer Number", "Trailer No", "Trailer ID", "Number"),
        Field(ImportFieldKeys.Type, "Trailer type", false, "Trailer Type", "Type", "Equipment", "Equipment Type", "Body Type"),
        Field(Vin, "VIN", false, "VIN", "VIN #", "VIN Number", "Serial Number"),
        Field(Make, "Make", false, "Make", "Manufacturer"),
        Field(Model, "Model", false, "Model"),
        Field(Year, "Year", false, "Year", "Model Year", "Yr"),
        Field(LicensePlate, "License plate", false, "Plate", "Plate #", "Plate Number", "License Plate", "License Plate #",
            "Tag", "Tag #", "Registration"),
        Field(LicensePlateState, "Plate state / region", false, "Plate State", "License State", "License Plate State",
            "Registration State", "Tag State", "State"),
        Field(Length, "Length (ft for US tenants, m for EU; a header saying ft or m wins)", false, "Length",
            "Length (ft)", "Length ft", "Length (feet)", "Length (m)", "Length m", "Trailer Length", "Size"),
        Field(Status, "Status", false, "Status", "Trailer Status"),
        Field(Notes, "Notes", false, "Notes", "Note", "Comments", "Memo")
    ];

    private static readonly IReadOnlyList<ImportField> Loads =
    [
        Field(Reference, "Load reference", true, "Load", "Load #", "Load Number", "Load No", "Load ID", "Pro",
            "Pro #", "Pro Number", "Reference", "Reference #", "Reference Number", "Ref", "Ref #", "Order",
            "Order #", "Order Number", "Shipment", "Shipment #", "Shipment ID", "Trip #"),
        Field(Customer, "Customer (name)", true, "Customer", "Customer Name", "Broker", "Broker Name", "Bill To",
            "Company", "Client"),
        Field(Rate, "Rate", true, "Rate", "Total Rate", "Amount", "Total", "Line Haul", "Linehaul", "Revenue",
            "Price", "Load Pay", "Pay", "Gross"),
        Field(Status, "Status", false, "Status", "Load Status"),
        Field(ImportFieldKeys.Type, "Load type", false, "Load Type", "Freight Type", "Type"),
        Field(Truck, "Truck number", false, "Truck", "Truck #", "Truck Number", "Unit", "Unit #", "Unit Number",
            "Tractor", "Tractor #"),
        Field(Driver, "Driver (name or email)", false, "Driver", "Driver Name", "Driver Email", "Assigned Driver"),
        Field(PickupDate, "Pickup date", false, "Pickup Date", "Pick Up Date", "PU Date", "Ship Date",
            "Origin Date", "Pickup Appointment", "Start Date"),
        Field(DeliveryDate, "Delivery date", false, "Delivery Date", "Del Date", "Drop Date", "Drop Off Date",
            "Destination Date", "Delivery Appointment", "End Date"),
        Field(Notes, "Notes", false, "Notes", "Note", "Comments", "Memo", "Instructions"),
        .. AddressFields(PickupPrefix, "Pickup ", PickupPrefixes),
        .. AddressFields(DeliveryPrefix, "Delivery ", DeliveryPrefixes)
    ];

    public static IReadOnlyList<ImportField> For(ImportEntityType entityType) => entityType switch
    {
        ImportEntityType.Customers => Customers,
        ImportEntityType.Drivers => Drivers,
        ImportEntityType.Trucks => Trucks,
        ImportEntityType.Trailers => Trailers,
        ImportEntityType.Loads => Loads,
        _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null)
    };

    private static ImportField Field(string key, string label, bool required, params string[] synonyms) =>
        new(key, label, required, synonyms);

    /// <summary>
    ///     Street / city / state / zip / country plus a single-column fallback. Prefixed groups (pickup,
    ///     delivery) also accept every base spelling behind each prefix ("Origin City", "Shipper Zip").
    /// </summary>
    private static IEnumerable<ImportField> AddressFields(string? prefix, string labelPrefix, string[]? synonymPrefixes = null)
    {
        (string Part, string Label, string[] Bases)[] parts =
        [
            (Street, "street", ["Address", "Address 1", "Address Line 1", "Street", "Street Address", "Address1", "Line 1"]),
            (Street2, "street line 2", ["Address 2", "Address Line 2", "Suite", "Address2", "Line 2", "Unit / Suite"]),
            (City, "city", ["City", "Town"]),
            (State, "state / region", ["State", "Province", "Region", "ST", "State/Province"]),
            (Zip, "zip / postal code", ["Zip", "Zip Code", "ZIP/Postal", "Postal Code", "Postcode", "Post Code"]),
            (Country, "country", ["Country", "Country Code"]),
            (FullAddress, "full address (parsed best-effort)", ["Full Address", "Location", "Address Full"])
        ];

        foreach (var (part, label, bases) in parts)
        {
            var synonyms = synonymPrefixes is null
                ? bases
                : [.. synonymPrefixes.SelectMany(p => bases.Select(b => $"{p} {b}")),
                   .. part == FullAddress ? synonymPrefixes : Array.Empty<string>()];

            var key = ImportFieldKeys.Address(prefix, part);
            var fullLabel = string.IsNullOrEmpty(labelPrefix)
                ? char.ToUpperInvariant(label[0]) + label[1..]
                : labelPrefix + label;
            yield return new ImportField(key, fullLabel, false, synonyms);
        }
    }
}

namespace Logistics.Domain.Entities;

/// <summary>
/// One vehicle carried by a <see cref="Primitives.Enums.LoadType.Vehicle"/> load. Stored as JSON on
/// the load row, so reading a load's vehicles costs no extra query. <see cref="Id"/> is stable across
/// edits and is what a <see cref="LoadConditionReport"/> points at.
/// </summary>
public class LoadVehicle
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string? Vin { get; set; }
    public int? Year { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public string? Color { get; set; }

    /// <summary>
    /// Auction lot, dealer stock or order number the shipper uses to identify the unit.
    /// </summary>
    public string? LotNumber { get; set; }

    /// <summary>
    /// False for an inoperable unit that needs a winch or forklift to load.
    /// </summary>
    public bool IsOperable { get; set; } = true;

    public string? Notes { get; set; }

    /// <summary>
    /// Optional per-vehicle rate, in the load's <see cref="Load.DeliveryCost"/> currency.
    /// </summary>
    public decimal? Rate { get; set; }

    /// <summary>
    /// "2021 Toyota Camry", falling back to the VIN or lot number when the description is missing.
    /// </summary>
    public string GetDisplayName()
    {
        var description = string.Join(' ', new[] { Year?.ToString(), Make, Model }
            .Where(p => !string.IsNullOrWhiteSpace(p)));

        if (!string.IsNullOrEmpty(description))
        {
            return description;
        }

        return Vin ?? LotNumber ?? "Vehicle";
    }
}

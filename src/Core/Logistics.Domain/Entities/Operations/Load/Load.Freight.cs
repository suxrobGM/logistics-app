using Logistics.Domain.Primitives.Enums;

namespace Logistics.Domain.Entities;

/// <summary>
/// Shipment details LTL carriers rate and load by. All optional; full truckloads usually leave them blank.
/// </summary>
public partial class Load
{
    public FreightClass? FreightClass { get; set; }

    /// <summary>NMFC item number, e.g. "156600" or "156600-03".</summary>
    public string? NmfcCode { get; set; }

    public int? PalletCount { get; set; }
    public int? PieceCount { get; set; }

    /// <summary>Gross weight in the tenant's weight unit.</summary>
    public double? Weight { get; set; }

    /// <summary>Free-text dimensions as the shipper states them, e.g. "48x40x60 in".</summary>
    public string? Dimensions { get; set; }

    public bool IsStackable { get; set; }
}

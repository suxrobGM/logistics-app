using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Services;

/// <summary>
/// LTL freight details and the charges billed on top of the linehaul, for a load being created.
/// </summary>
public record LoadShipmentDetails(
    decimal FuelSurcharge = 0,
    IReadOnlyList<LoadAccessorialDto>? Accessorials = null,
    FreightClass? FreightClass = null,
    string? NmfcCode = null,
    int? PalletCount = null,
    int? PieceCount = null,
    double? Weight = null,
    string? Dimensions = null,
    bool IsStackable = false)
{
    /// <summary>Copies the details onto a new load and rebuilds its invoice lines to match.</summary>
    public void ApplyTo(Load load)
    {
        load.FuelSurcharge = FuelSurcharge;
        load.FreightClass = FreightClass;
        load.NmfcCode = NmfcCode;
        load.PalletCount = PalletCount;
        load.PieceCount = PieceCount;
        load.Weight = Weight;
        load.Dimensions = Dimensions;
        load.IsStackable = IsStackable;
        load.Accessorials = (Accessorials ?? []).Select(a => LoadChargeService.NewAccessorial(load, a)).ToList();

        if (load.Invoice is { } invoice)
        {
            invoice.ReplaceLoadCharges(load);
            invoice.RecalculateTotals();
        }
    }
}

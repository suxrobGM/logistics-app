using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

/// <summary>
///     The charge and LTL fields shared by create and update, so both validate alike.
/// </summary>
public interface ILoadChargeFields
{
    decimal? FuelSurcharge { get; }
    List<LoadAccessorialDto>? Accessorials { get; }
    string? NmfcCode { get; }
    int? PalletCount { get; }
    int? PieceCount { get; }
    double? Weight { get; }
    string? Dimensions { get; }
}

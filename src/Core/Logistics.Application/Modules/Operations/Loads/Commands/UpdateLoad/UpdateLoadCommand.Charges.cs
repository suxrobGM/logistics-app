using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

public partial class UpdateLoadCommand : ILoadChargeFields
{
    public decimal? FuelSurcharge { get; set; }

    /// <summary>
    /// Replaces the load's accessorials when sent (keep a row by sending its id); null leaves them.
    /// Changing charges rebuilds the invoice's load lines unless the invoice is paid or cancelled.
    /// </summary>
    public List<LoadAccessorialDto>? Accessorials { get; set; }

    public FreightClass? FreightClass { get; set; }
    public string? NmfcCode { get; set; }
    public int? PalletCount { get; set; }
    public int? PieceCount { get; set; }
    public double? Weight { get; set; }
    public string? Dimensions { get; set; }
    public bool? IsStackable { get; set; }
}

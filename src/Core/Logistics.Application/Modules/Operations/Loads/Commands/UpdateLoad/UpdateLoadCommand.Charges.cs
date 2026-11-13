using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

public partial class UpdateLoadCommand : ILoadChargeFields
{
    public decimal? FuelSurcharge { get; set; }

    /// <summary>Accessorial charges for the load; null leaves them unchanged. A change rebuilds the load's invoice lines unless the invoice is paid or cancelled.</summary>
    public List<LoadAccessorialDto>? Accessorials { get; set; }

    public FreightClass? FreightClass { get; set; }
    public string? NmfcCode { get; set; }
    public int? PalletCount { get; set; }
    public int? PieceCount { get; set; }
    public decimal? WeightKg { get; set; }
    public string? Dimensions { get; set; }
    public bool? IsStackable { get; set; }
}

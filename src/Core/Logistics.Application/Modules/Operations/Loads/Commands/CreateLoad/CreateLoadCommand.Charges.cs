using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Loads.Commands;

public partial class CreateLoadCommand
{
    public decimal FuelSurcharge { get; set; }
    public List<LoadAccessorialDto> Accessorials { get; set; } = [];

    public FreightClass? FreightClass { get; set; }
    public string? NmfcCode { get; set; }
    public int? PalletCount { get; set; }
    public int? PieceCount { get; set; }
    public double? Weight { get; set; }
    public string? Dimensions { get; set; }
    public bool IsStackable { get; set; }
}

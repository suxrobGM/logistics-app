using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trailers.Commands;

public class CreateTrailerCommand : ICommand<Result<TrailerDto>>, ITrailerFields
{
    public string Number { get; set; } = null!;
    public TrailerType Type { get; set; }
    public TrailerStatus Status { get; set; } = TrailerStatus.Active;
    public string? Vin { get; set; }
    public string? LicensePlate { get; set; }
    public string? LicensePlateState { get; set; }
    public int? Year { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public decimal? LengthMeters { get; set; }
    public string? Notes { get; set; }
}

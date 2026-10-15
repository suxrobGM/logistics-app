using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Operations.Trailers.Commands;

/// <summary>
///     Replaces every editable field, so a null clears the optional ones.
/// </summary>
public class UpdateTrailerCommand : ICommand<Result>, ITrailerFields
{
    public Guid Id { get; set; }
    public string Number { get; set; } = null!;
    public TrailerType Type { get; set; }
    public TrailerStatus Status { get; set; }
    public string? Vin { get; set; }
    public string? LicensePlate { get; set; }
    public string? LicensePlateState { get; set; }
    public int? Year { get; set; }
    public string? Make { get; set; }
    public string? Model { get; set; }
    public decimal? LengthMeters { get; set; }
    public string? Notes { get; set; }
}

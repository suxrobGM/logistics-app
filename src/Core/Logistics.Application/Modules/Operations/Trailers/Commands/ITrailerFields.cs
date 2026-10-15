using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Operations.Trailers.Commands;

/// <summary>
///     The editable trailer fields shared by create and update, so both validate alike.
/// </summary>
public interface ITrailerFields
{
    string Number { get; }
    TrailerType Type { get; }
    TrailerStatus Status { get; }
    string? Vin { get; }
    string? LicensePlate { get; }
    string? LicensePlateState { get; }
    int? Year { get; }
    string? Make { get; }
    string? Model { get; }
    decimal? LengthMeters { get; }
    string? Notes { get; }
}

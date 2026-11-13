namespace Logistics.Application.Modules.AI.Negotiation.Commands;

/// <summary>
/// The lane and rate fields shared by the create and update commands, so both inherit one set of
/// validation rules.
/// </summary>
public interface ILaneRateFloorFields
{
    string OriginCountry { get; }
    string? OriginState { get; }
    string DestinationCountry { get; }
    string? DestinationState { get; }
    decimal MinRatePerMile { get; }
    decimal? MinTotalRate { get; }
    string? Notes { get; }
}

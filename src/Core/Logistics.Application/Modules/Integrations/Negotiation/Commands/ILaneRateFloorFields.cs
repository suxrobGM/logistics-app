namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

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
    decimal? MinTotalRateAmount { get; }
    string? Notes { get; }
}

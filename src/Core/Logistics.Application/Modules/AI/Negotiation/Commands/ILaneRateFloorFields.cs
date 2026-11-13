namespace Logistics.Application.Modules.AI.Negotiation.Commands;

public interface ILaneRateFloorFields
{
    string OriginCountry { get; }
    string? OriginState { get; }
    string DestinationCountry { get; }
    string? DestinationState { get; }
    decimal MinRatePerKm { get; }
    decimal? MinTotalRate { get; }
    string? Notes { get; }
}

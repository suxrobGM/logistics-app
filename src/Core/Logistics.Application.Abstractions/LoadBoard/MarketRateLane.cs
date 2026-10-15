using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Abstractions.LoadBoard;

/// <summary>
/// A city-to-city lane to price against the board's market rates.
/// </summary>
public sealed record MarketRateLane(
    string OriginCity,
    string OriginState,
    string DestinationCity,
    string DestinationState,
    LoadBoardEquipmentType? EquipmentType);

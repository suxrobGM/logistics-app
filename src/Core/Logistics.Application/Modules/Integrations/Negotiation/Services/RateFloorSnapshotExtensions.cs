using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Services;

public static class RateFloorSnapshotExtensions
{
    /// <summary>
    /// Reduces a resolved floor to what a negotiation stores. A per-mile-only floor is converted to
    /// a total using the listing's distance, so a thread opened on a per-mile floor still has a
    /// total to check bookings against.
    /// </summary>
    public static RateFloorSnapshot ToSnapshot(this EffectiveRateFloorDto floor, string currency)
    {
        if (!floor.HasFloor)
        {
            return RateFloorSnapshot.None;
        }

        var total = floor.MinTotalRate ?? (floor.EffectiveFloorTotal is { } amount
            ? new Money { Amount = amount, Currency = currency }
            : null);

        return new RateFloorSnapshot(floor.MinRatePerMile, total, floor.Source);
    }
}

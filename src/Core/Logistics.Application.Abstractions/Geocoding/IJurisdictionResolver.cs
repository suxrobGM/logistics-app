using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Abstractions.Geocoding;

/// <summary>
/// Resolves a GPS point to a tax jurisdiction (country + state/province) using offline
/// boundary polygons — no per-ping API cost. Country-agnostic: coverage grows by adding
/// polygons (US + CA today; EU road-toll reporting later).
/// </summary>
public interface IJurisdictionResolver
{
    /// <summary>
    /// Returns the jurisdiction containing the point, or null when the point falls outside
    /// all known boundaries (offshore, GPS glitch, uncovered country).
    /// </summary>
    TaxJurisdiction? Resolve(GeoPoint point);
}

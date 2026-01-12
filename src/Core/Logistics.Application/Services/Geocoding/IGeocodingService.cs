using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Application.Services.Geocoding;

/// <summary>
/// Service for geocoding addresses to coordinates.
/// </summary>
public interface IGeocodingService
{
    /// <summary>
    /// Geocodes an address to a geographical point.
    /// </summary>
    /// <param name="street">Street address.</param>
    /// <param name="city">City name.</param>
    /// <param name="state">State/province code.</param>
    /// <param name="zipCode">Postal code.</param>
    /// <param name="country">Country code (default: USA).</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>GeoPoint with longitude and latitude, or error.</returns>
    Task<Result<GeoPoint>> GeocodeAddressAsync(
        string street,
        string city,
        string state,
        string? zipCode,
        string country = "USA",
        CancellationToken ct = default);
}

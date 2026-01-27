using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Web;
using Logistics.Application.Services.Geocoding;
using Logistics.Domain.Primitives.ValueObjects;
using Logistics.Infrastructure.Options;
using Logistics.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Routing.Geocoding;

/// <summary>
///     Geocoding service using Mapbox Geocoding API.
/// </summary>
public sealed class MapboxGeocodingService(
    HttpClient http,
    IOptions<MapboxOptions> options,
    ILogger<MapboxGeocodingService> logger)
    : IGeocodingService
{
    private readonly MapboxOptions options = options.Value;

    public async Task<Result<GeoPoint>> GeocodeAddressAsync(
        string line1,
        string city,
        string state,
        string? zipCode = null,
        string country = "USA",
        CancellationToken ct = default)
    {
        try
        {
            // Build the search query
            var addressParts = new List<string> { line1, city, state };
            if (!string.IsNullOrWhiteSpace(zipCode))
            {
                addressParts.Add(zipCode);
            }

            addressParts.Add(country);

            var searchText = string.Join(", ", addressParts);
            var encodedSearch = HttpUtility.UrlEncode(searchText);

            // Call Mapbox Geocoding API
            var url = $"https://api.mapbox.com/geocoding/v5/mapbox.places/{encodedSearch}.json" +
                      $"?access_token={options.AccessToken}" +
                      "&limit=1" +
                      "&types=address,place";

            logger.LogDebug("Geocoding address: {Address}", searchText);

            var response = await http.GetFromJsonAsync<MapboxGeocodingResponse>(url, ct);

            if (response?.Features is null || response.Features.Count == 0)
            {
                logger.LogWarning("No geocoding results found for address: {Address}", searchText);
                return Result<GeoPoint>.Fail($"No location found for address: {searchText}");
            }

            var feature = response.Features[0];
            var coordinates = feature.Center;

            if (coordinates is null || coordinates.Length < 2)
            {
                return Result<GeoPoint>.Fail("Invalid coordinates returned from geocoding service");
            }

            // Mapbox returns [longitude, latitude]
            var geoPoint = new GeoPoint(coordinates[0], coordinates[1]);

            logger.LogDebug("Geocoded {Address} to ({Lat}, {Lng})",
                searchText, geoPoint.Latitude, geoPoint.Longitude);

            return Result<GeoPoint>.Ok(geoPoint);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex, "HTTP error during geocoding");
            return Result<GeoPoint>.Fail($"Geocoding service error: {ex.Message}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during geocoding");
            return Result<GeoPoint>.Fail($"Geocoding error: {ex.Message}");
        }
    }

    private sealed class MapboxGeocodingResponse
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("features")]
        public List<MapboxFeature>? Features { get; set; }
    }

    private sealed class MapboxFeature
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("place_name")]
        public string? PlaceName { get; set; }

        [JsonPropertyName("center")]
        public double[]? Center { get; set; }

        [JsonPropertyName("relevance")]
        public double? Relevance { get; set; }
    }
}

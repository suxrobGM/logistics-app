using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Logistics.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace Logistics.Infrastructure.Services.Trip;

public sealed class MapboxMatrixClient
{
    private readonly HttpClient _http;
    private readonly MapboxOptions _opt;

    public MapboxMatrixClient(HttpClient http, IOptions<MapboxOptions> opt)
    {
        _http = http;
        _opt = opt.Value;
    }

    public async Task<MapboxMatrixResult> GetMatrixAsync(IReadOnlyList<(double lng, double lat)> coords,
        CancellationToken ct)
    {
        // limit guard
        if (coords.Count > _opt.MaxCoordsPerRequest)
        {
            throw new InvalidOperationException(
                $"Too many coordinates for Matrix request: {coords.Count} > max {_opt.MaxCoordsPerRequest}");
        }

        var coordStr = string.Join(';',
            coords.Select(c =>
                $"{c.lng.ToString(CultureInfo.InvariantCulture)},{c.lat.ToString(CultureInfo.InvariantCulture)}"));

        var url =
            $"https://api.mapbox.com/directions-matrix/v1/mapbox/driving/{coordStr}?annotations=distance,duration&access_token={_opt.AccessToken}";

        var rsp = await _http.GetFromJsonAsync<MapboxMatrixResponse>(url, ct);
        if (rsp is null || rsp.Code != "Ok" || rsp.Durations is null || rsp.Distances is null)
        {
            throw new InvalidOperationException($"Mapbox Matrix error: {rsp?.Code ?? "null"}");
        }

        return new MapboxMatrixResult(rsp.Durations, rsp.Distances);
    }

    private sealed class MapboxMatrixResponse
    {
        [JsonPropertyName("code")] public string Code { get; set; } = "";
        [JsonPropertyName("durations")] public double?[][]? Durations { get; set; }
        [JsonPropertyName("distances")] public double?[][]? Distances { get; set; }
    }
}

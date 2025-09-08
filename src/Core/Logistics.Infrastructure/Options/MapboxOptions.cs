namespace Logistics.Infrastructure.Options;

public sealed class MapboxOptions
{
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    ///     API limit: most plans allow up to 25 coordinates per request.
    ///     Includes depot(s) + all stops.
    /// </summary>
    public int MaxCoordsPerRequest { get; set; } = 25;
}

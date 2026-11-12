namespace Logistics.Shared.Geo;

/// <summary>
/// Represents a country with its display name and code.
/// </summary>
/// <param name="DisplayName">Display name of the country.</param>
/// <param name="Code">ISO code of the country.</param>
public record Country(string DisplayName, string Code);

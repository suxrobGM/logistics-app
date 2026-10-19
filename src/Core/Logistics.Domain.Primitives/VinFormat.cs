namespace Logistics.Domain.Primitives;

public static class VinFormat
{
    /// <summary>
    /// The canonical form of a VIN: trimmed and upper-cased, blank as null. Trucks and trailers store only this,
    /// so their unique VIN indexes can't be defeated by case and lookups compare exactly.
    /// </summary>
    public static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToUpperInvariant();
}

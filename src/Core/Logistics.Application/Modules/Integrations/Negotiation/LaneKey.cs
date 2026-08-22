namespace Logistics.Application.Modules.Integrations.Negotiation;

/// <summary>
/// Lane country/state normalization. The write side and the resolver must normalize identically or
/// a saved floor never matches the listing it was meant to cover, so both go through here.
/// </summary>
public static class LaneKey
{
    public static string Country(string country) => country.Trim().ToUpperInvariant();

    /// <summary>Blank means "any state on this lane", which is stored as null.</summary>
    public static string? State(string? state) =>
        string.IsNullOrWhiteSpace(state) ? null : state.Trim().ToUpperInvariant();
}

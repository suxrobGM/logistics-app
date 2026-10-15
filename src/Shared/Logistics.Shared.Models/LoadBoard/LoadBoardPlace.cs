using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Shared.Models;

/// <summary>
/// A search or posting location as load boards take it: a city and state, or a postal code.
/// Load boards match on these, not on a street address.
/// </summary>
public record LoadBoardPlace
{
    private static readonly HashSet<string> CanadianProvinces =
        ["AB", "BC", "MB", "NB", "NL", "NS", "NT", "NU", "ON", "PE", "PQ", "QC", "SK", "YT"];

    public string? City { get; set; }

    /// <summary>
    /// Two-letter state or province code, e.g. "TX" or "ON".
    /// </summary>
    public string? State { get; set; }

    public string? PostalCode { get; set; }

    /// <summary>
    /// The place as an <see cref="Address"/>. Boards never give a street, so Line1 is empty, and
    /// so is the ZIP when the board did not send one - empty, never a placeholder that reads as
    /// real data.
    /// </summary>
    public Address ToAddress() => new()
    {
        Line1 = string.Empty,
        City = City ?? string.Empty,
        State = State ?? string.Empty,
        ZipCode = PostalCode ?? string.Empty,
        Country = State is not null && CanadianProvinces.Contains(State.ToUpperInvariant()) ? "CA" : "US"
    };

    public static LoadBoardPlace FromAddress(Address address) => new()
    {
        City = NullIfEmpty(address.City),
        State = NullIfEmpty(address.State),
        PostalCode = NullIfEmpty(address.ZipCode)
    };

    private static string? NullIfEmpty(string? value) => string.IsNullOrWhiteSpace(value) ? null : value;
}

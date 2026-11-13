using System.Collections.Frozen;
using Logistics.Domain.Primitives;
using Logistics.Domain.Primitives.Geo;

namespace Logistics.Infrastructure.Payments.Tax.Data;

/// <summary>
/// EU membership + reverse-charge eligibility check over <see cref="RegionCountries.EUMemberStates"/>
/// (EU-27) - kept narrow on purpose: only true EU member states get reverse-charge under Article 196
/// of Directive 2006/112/EC. EEA/UK/Western-Balkans countries are not in the EU VAT area.
/// </summary>
public static class EUVatRules
{
    private static readonly FrozenSet<string> EUMembers =
        RegionCountries.EUMemberStates.ToFrozenSet(StringComparer.OrdinalIgnoreCase);

    public static bool IsEUMember(string? countryCode) =>
        !string.IsNullOrEmpty(countryCode) && EUMembers.Contains(countryCode);

    /// <summary>
    /// EU cross-border B2B reverse charge applies when:
    /// 1) seller and buyer are in different EU member states,
    /// 2) the buyer supplied a valid VAT ID (we trust presence; full VIES check is plan #10),
    /// 3) buyer isn't VAT-exempt (charity, etc.).
    /// </summary>
    public static bool IsReverseCharge(string? sellerCountry, string? buyerCountry, string? buyerVatId)
    {
        if (string.IsNullOrWhiteSpace(sellerCountry) || string.IsNullOrWhiteSpace(buyerCountry)) return false;
        if (string.IsNullOrWhiteSpace(buyerVatId)) return false;
        if (!IsEUMember(sellerCountry) || !IsEUMember(buyerCountry)) return false;
        return !sellerCountry.Equals(buyerCountry, StringComparison.OrdinalIgnoreCase);
    }
}

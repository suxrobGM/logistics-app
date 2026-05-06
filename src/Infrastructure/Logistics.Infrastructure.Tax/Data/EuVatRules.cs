namespace Logistics.Infrastructure.Tax.Data;

/// <summary>
/// EU member-state set + reverse-charge eligibility check. Mirrors the backend
/// <c>RegionCountries</c> set in <c>Logistics.Domain</c> (EU-27) — kept narrow on purpose:
/// only true EU member states get reverse-charge under Article 196 of Directive 2006/112/EC.
/// EEA/UK/Western-Balkans countries handled separately as they are not in the EU VAT area.
/// </summary>
public static class EuVatRules
{
    /// <summary>EU-27 member states (ISO-2).</summary>
    public static readonly HashSet<string> EuMemberStates =
    [
        "AT", "BE", "BG", "HR", "CY", "CZ", "DK", "EE", "FI", "FR",
        "DE", "GR", "HU", "IE", "IT", "LV", "LT", "LU", "MT", "NL",
        "PL", "PT", "RO", "SK", "SI", "ES", "SE"
    ];

    public static bool IsEuMember(string? countryCode) =>
        !string.IsNullOrEmpty(countryCode) && EuMemberStates.Contains(countryCode.ToUpperInvariant());

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
        if (!IsEuMember(sellerCountry) || !IsEuMember(buyerCountry)) return false;
        return !sellerCountry.Equals(buyerCountry, StringComparison.OrdinalIgnoreCase);
    }
}

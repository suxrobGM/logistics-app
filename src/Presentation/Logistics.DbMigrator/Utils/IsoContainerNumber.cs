namespace Logistics.DbMigrator.Utils;

/// <summary>
/// Generates valid ISO 6346 container numbers with mod-11 checksum.
/// Format: 4 letters (3-letter owner code + category letter U/J/Z) + 6-digit serial + check digit.
/// Example: MSCU1234565 — owner MSC, category U (freight container), serial 123456, check 5.
/// </summary>
internal static class IsoContainerNumber
{
    // Letter→numeric values per ISO 6346. Numbers 11, 22, 33 are skipped (excluded for divisibility by 11).
    private static readonly Dictionary<char, int> LetterValues = new()
    {
        ['A'] = 10, ['B'] = 12, ['C'] = 13, ['D'] = 14, ['E'] = 15, ['F'] = 16, ['G'] = 17,
        ['H'] = 18, ['I'] = 19, ['J'] = 20, ['K'] = 21, ['L'] = 23, ['M'] = 24, ['N'] = 25,
        ['O'] = 26, ['P'] = 27, ['Q'] = 28, ['R'] = 29, ['S'] = 30, ['T'] = 31, ['U'] = 32,
        ['V'] = 34, ['W'] = 35, ['X'] = 36, ['Y'] = 37, ['Z'] = 38
    };

    /// <summary>
    /// Generates a valid ISO 6346 container number for the given 3-letter owner code.
    /// Category is fixed to U (freight container), which is the standard for cargo containers.
    /// </summary>
    public static string Generate(string ownerCode, Random random)
    {
        ArgumentNullException.ThrowIfNull(ownerCode);
        if (ownerCode.Length != 3)
        {
            throw new ArgumentException("Owner code must be 3 letters.", nameof(ownerCode));
        }

        var serial = random.Next(0, 1_000_000).ToString("D6");
        var prefix = ownerCode.ToUpperInvariant() + "U" + serial;
        var checkDigit = ComputeCheckDigit(prefix);
        return prefix + checkDigit;
    }

    private static int ComputeCheckDigit(string prefix)
    {
        // Sum of (value × 2^position) for the 10-character prefix, mod 11, mod 10.
        var total = 0;
        for (var i = 0; i < 10; i++)
        {
            var c = prefix[i];
            var value = char.IsLetter(c) ? LetterValues[c] : c - '0';
            total += value * (1 << i);
        }
        return total % 11 % 10;
    }
}

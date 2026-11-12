using System.Globalization;

namespace Logistics.Domain.Primitives;

/// <summary>
/// Since .NET 11 a <c>(decimal)</c> cast keeps the exact binary value (<c>(decimal)0.3f</c> is
/// 0.300000011920928955078125), so binary noise leaks into money math and flips midpoint rounding.
/// Use <c>ToDecimal()</c> instead of the cast for any value that reaches a monetary or stored figure.
/// </summary>
public static class FloatingPointExtensions
{
    extension(double value)
    {
        /// <summary>Converts at 15 significant digits, the pre-.NET 11 cast behaviour.</summary>
        public decimal ToDecimal() => ParseDecimal(value.ToString("G15", CultureInfo.InvariantCulture));
    }

    extension(float value)
    {
        /// <summary>Converts at 7 significant digits, the pre-.NET 11 cast behaviour.</summary>
        public decimal ToDecimal() => ParseDecimal(value.ToString("G7", CultureInfo.InvariantCulture));
    }

    private static decimal ParseDecimal(string text) =>
        decimal.Parse(text, NumberStyles.Float, CultureInfo.InvariantCulture);
}

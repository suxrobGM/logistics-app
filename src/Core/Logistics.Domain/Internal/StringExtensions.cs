namespace Logistics.Domain;

internal static class StringExtensions
{
    internal static string NewGuid(this string str)
        => Guid.NewGuid().ToString();
}
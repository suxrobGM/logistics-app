namespace Logistics.Application.Shared;

public static class StringExtensions
{
    public static string Capitalize(this string? str)
    {
        if (string.IsNullOrEmpty(str))
            return string.Empty;

        return string.Concat(str[0].ToString().ToUpper(), str.AsSpan(1));
    }
}
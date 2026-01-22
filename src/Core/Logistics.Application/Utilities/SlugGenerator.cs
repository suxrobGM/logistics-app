using System.Text.RegularExpressions;

namespace Logistics.Application.Utilities;

public static partial class SlugGenerator
{
    public static string Generate(string title)
    {
        var slug = title.ToLowerInvariant();
        slug = SlugRegex().Replace(slug, "-");
        slug = MultipleHyphensRegex().Replace(slug, "-");
        slug = slug.Trim('-');
        return slug;
    }

    [GeneratedRegex(@"[^a-z0-9\s-]")]
    private static partial Regex SlugRegex();

    [GeneratedRegex(@"-+")]
    private static partial Regex MultipleHyphensRegex();
}

using System.Xml.Linq;
using Xunit;

namespace Logistics.Architecture.Tests;

internal static class CsprojAssertions
{
    public static void AssertNoPackage(string csprojPath, string packageName)
    {
        var doc = XDocument.Load(csprojPath);
        var hit = doc.Descendants("PackageReference")
            .Any(e => string.Equals(
                (string?)e.Attribute("Include"),
                packageName,
                StringComparison.OrdinalIgnoreCase));

        Assert.False(hit,
            $"{Path.GetFileName(csprojPath)} unexpectedly references PackageReference '{packageName}'.");
    }

    public static void AssertNoProjectReference(string csprojPath, string referencedProjectFileName)
    {
        var doc = XDocument.Load(csprojPath);
        var hit = doc.Descendants("ProjectReference")
            .Any(e =>
            {
                var include = (string?)e.Attribute("Include") ?? string.Empty;
                return include.Replace('\\', '/').EndsWith(
                    "/" + referencedProjectFileName,
                    StringComparison.OrdinalIgnoreCase);
            });

        Assert.False(hit,
            $"{Path.GetFileName(csprojPath)} unexpectedly references ProjectReference '{referencedProjectFileName}'.");
    }

    /// <summary>
    /// Walks up from the test bin directory until it finds <c>Logistics.slnx</c>,
    /// then joins the supplied relative segments. Lets the tests run regardless of
    /// where <c>dotnet test</c> is invoked from.
    /// </summary>
    public static string ResolveRepoFile(params string[] relative)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "Logistics.slnx")))
        {
            dir = dir.Parent;
        }

        if (dir is null)
        {
            throw new InvalidOperationException(
                "Could not find repo root (Logistics.slnx) walking up from " + AppContext.BaseDirectory);
        }

        var parts = new[] { dir.FullName }.Concat(relative).ToArray();
        return Path.Combine(parts);
    }
}

using Xunit;
using Logistics.Application.Modules.Integrations.Webhooks.Commands;

namespace Logistics.Architecture.Tests;

/// <summary>
/// XML-level rules over .csproj files - catches package/project references that
/// NetArchTest's IL inspection can't see (e.g., an unused PackageReference still
/// declared in the csproj).
///
/// Some assertions are <c>[Fact(Skip = ...)]</c> until slice 1.8 / 1.9-AI land.
/// See <c>.claude/plans/refactor-application-layer/REFACTOR-INDEX.md</c>.
/// </summary>
public class CsprojReferenceTests
{
    [Fact]
    public void Application_csproj_has_no_AspNetCore_Http_package()
    {
        var csproj = CsprojAssertions.ResolveRepoFile(
            "src", "Core", "Logistics.Application", "Logistics.Application.csproj");
        CsprojAssertions.AssertNoPackage(csproj, "Microsoft.AspNetCore.Http");
    }

    [Fact(Skip = "Re-enable when slice 1.8 lands; ProcessStripEventHandler still consumes raw Stripe.Event payloads.")]
    public void Application_csproj_has_no_Stripe_package()
    {
        var csproj = CsprojAssertions.ResolveRepoFile(
            "src", "Core", "Logistics.Application", "Logistics.Application.csproj");
        CsprojAssertions.AssertNoPackage(csproj, "Stripe.net");
    }

    [Fact]
    public void Abstractions_csproj_has_no_EFCore_package()
    {
        var csproj = CsprojAssertions.ResolveRepoFile(
            "src", "Core", "Logistics.Application.Abstractions", "Logistics.Application.Abstractions.csproj");
        CsprojAssertions.AssertNoPackage(csproj, "Microsoft.EntityFrameworkCore");
    }

    [Fact(Skip = "Re-enable when slice 1.8 lands; Abstractions still ships Stripe.net to support port signatures that take Stripe SDK types.")]
    public void Abstractions_csproj_has_no_Stripe_package()
    {
        var csproj = CsprojAssertions.ResolveRepoFile(
            "src", "Core", "Logistics.Application.Abstractions", "Logistics.Application.Abstractions.csproj");
        CsprojAssertions.AssertNoPackage(csproj, "Stripe.net");
    }

    /// <summary>
    /// Every project under src/Infrastructure, discovered from disk rather than hand-listed, so a
    /// newly added infrastructure project is covered by this rule without anyone remembering to
    /// register it here.
    /// </summary>
    public static TheoryData<string> InfrastructureProjects
    {
        get
        {
            // Logistics.Infrastructure.AI is intentionally exempt until slice 1.9-AI
            // decouples it. See REFACTOR-INDEX.md.
            string[] exempt = ["Logistics.Infrastructure.AI"];

            var root = CsprojAssertions.ResolveRepoFile("src", "Infrastructure");
            var data = new TheoryData<string>();

            foreach (var dir in Directory.EnumerateDirectories(root).Order())
            {
                var name = Path.GetFileName(dir);
                if (!exempt.Contains(name) && File.Exists(Path.Combine(dir, $"{name}.csproj")))
                {
                    data.Add(name);
                }
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(InfrastructureProjects))]
    public void Each_Infrastructure_csproj_does_not_reference_Application_project(string projectName)
    {
        var csproj = CsprojAssertions.ResolveRepoFile(
            "src", "Infrastructure", projectName, $"{projectName}.csproj");
        CsprojAssertions.AssertNoProjectReference(csproj, "Logistics.Application.csproj");
    }

    /// <summary>
    /// Every csproj in the repo, discovered from disk so a new project is covered without anyone
    /// remembering to register it. Skips <c>private/</c>, which may be an empty submodule in CI.
    /// </summary>
    public static TheoryData<string> AllProjects
    {
        get
        {
            var root = Path.GetDirectoryName(CsprojAssertions.ResolveRepoFile("Logistics.slnx"))!;
            var sep = Path.DirectorySeparatorChar;
            var data = new TheoryData<string>();

            foreach (var csproj in Directory
                         .EnumerateFiles(root, "*.csproj", SearchOption.AllDirectories)
                         .Where(p => !p.Contains($"{sep}private{sep}", StringComparison.Ordinal)
                                     && !p.Contains($"{sep}node_modules{sep}", StringComparison.Ordinal))
                         .Order(StringComparer.Ordinal))
            {
                data.Add(csproj);
            }

            return data;
        }
    }

    /// <summary>
    /// MediatR 13+ ships under a commercial licence or RPL-1.5 copyleft, neither of which suits a
    /// closed-source product. Logistics.Mediator replaced it; this keeps it replaced.
    /// </summary>
    [Theory]
    [MemberData(nameof(AllProjects))]
    public void No_csproj_references_the_commercially_licensed_MediatR_packages(string csproj)
    {
        CsprojAssertions.AssertNoPackage(csproj, "MediatR");
        CsprojAssertions.AssertNoPackage(csproj, "MediatR.Contracts");
    }
}

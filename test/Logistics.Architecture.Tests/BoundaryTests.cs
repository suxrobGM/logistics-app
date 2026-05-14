using System.Reflection;
using NetArchTest.Rules;
using Xunit;

namespace Logistics.Architecture.Tests;

/// <summary>
/// Layering rules over compiled assemblies.
///
/// Layering checks against other Logistics.* assemblies use <c>GetReferencedAssemblies()</c>
/// (exact-name matching), because NetArchTest's <c>HaveDependencyOn</c> matches by prefix
/// and would falsely flag <c>Logistics.Application.Abstractions</c> on a rule meant only
/// for <c>Logistics.Application</c>.
///
/// Checks against external SDKs (Stripe, EF Core, AspNetCore.Http) use NetArchTest because
/// those names don't overlap with anything we ship.
///
/// Some assertions are <c>[Fact(Skip = ...)]</c> because slice 1.8 (Stripe SDK→DTO replacement)
/// and slice 1.9-AI (Infrastructure.AI decoupling) are deferred. Re-enable when those land.
/// See <c>.claude/plans/refactor-application-layer/REFACTOR-INDEX.md</c>.
/// </summary>
public class BoundaryTests
{
    private static readonly string[] AllInfrastructureAssemblies =
    [
        "Logistics.Infrastructure.AI",
        "Logistics.Infrastructure.Communications",
        "Logistics.Infrastructure.Documents",
        "Logistics.Infrastructure.Integrations.Eld",
        "Logistics.Infrastructure.Integrations.LoadBoard",
        "Logistics.Infrastructure.Payments",
        "Logistics.Infrastructure.Persistence",
        "Logistics.Infrastructure.Routing",
        "Logistics.Infrastructure.Storage",
        "Logistics.Infrastructure.Tax",
        "Logistics.Infrastructure.Vin"
    ];

    [Fact]
    public void Application_must_not_depend_on_AspNetCore_Http()
    {
        var result = Types.InAssembly(AssemblyAnchors.Application.Assembly)
            .ShouldNot().HaveDependencyOn("Microsoft.AspNetCore.Http")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact(Skip = "Re-enable when slice 1.8 lands; ProcessStripEventHandler still consumes raw Stripe.Event payloads.")]
    public void Application_must_not_depend_on_Stripe()
    {
        var result = Types.InAssembly(AssemblyAnchors.Application.Assembly)
            .ShouldNot().HaveDependencyOn("Stripe")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact]
    public void Application_must_not_reference_Infrastructure_assemblies()
    {
        var hits = ReferencedAssemblyNames(AssemblyAnchors.Application.Assembly)
            .Intersect(AllInfrastructureAssemblies, StringComparer.OrdinalIgnoreCase)
            .ToList();

        Assert.True(hits.Count == 0,
            "Logistics.Application directly references forbidden Infrastructure assemblies: " +
            string.Join(", ", hits));
    }

    [Fact]
    public void Abstractions_must_not_reference_Application_or_Infrastructure_assemblies()
    {
        // Exact-name match via reflection — NetArchTest's prefix matching can't safely separate
        // "Logistics.Application" from "Logistics.Application.Abstractions" itself.
        var refs = ReferencedAssemblyNames(AssemblyAnchors.ApplicationAbstractions.Assembly).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var forbidden = new[] { "Logistics.Application" }
            .Concat(AllInfrastructureAssemblies)
            .ToList();

        var hits = forbidden.Where(refs.Contains).ToList();
        Assert.True(hits.Count == 0,
            "Logistics.Application.Abstractions references forbidden assemblies: " +
            string.Join(", ", hits));
    }

    [Fact]
    public void Abstractions_must_not_depend_on_EFCore_or_AspNetCore_Http()
    {
        var result = Types.InAssembly(AssemblyAnchors.ApplicationAbstractions.Assembly)
            .ShouldNot().HaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore.Http")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    [Fact(Skip = "Re-enable when slice 1.8 lands; Abstractions still ships Stripe.net to support port signatures that take Stripe SDK types.")]
    public void Abstractions_must_not_depend_on_Stripe()
    {
        var result = Types.InAssembly(AssemblyAnchors.ApplicationAbstractions.Assembly)
            .ShouldNot().HaveDependencyOn("Stripe")
            .GetResult();

        Assert.True(result.IsSuccessful, FormatFailures(result));
    }

    // Logistics.Infrastructure.AI is intentionally omitted from this Theory
    // until slice 1.9-AI decouples it. See REFACTOR-INDEX.md.
    [Theory]
    [InlineData("Logistics.Infrastructure.Communications")]
    [InlineData("Logistics.Infrastructure.Documents")]
    [InlineData("Logistics.Infrastructure.Integrations.Eld")]
    [InlineData("Logistics.Infrastructure.Integrations.LoadBoard")]
    [InlineData("Logistics.Infrastructure.Payments")]
    [InlineData("Logistics.Infrastructure.Persistence")]
    [InlineData("Logistics.Infrastructure.Routing")]
    [InlineData("Logistics.Infrastructure.Storage")]
    [InlineData("Logistics.Infrastructure.Tax")]
    [InlineData("Logistics.Infrastructure.Vin")]
    public void Each_Infrastructure_assembly_references_Abstractions_not_Application(string assemblyName)
    {
        var asm = AssemblyAnchors.LoadByName(assemblyName);
        var refs = ReferencedAssemblyNames(asm).ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Exact match — Logistics.Application.Abstractions is allowed; only the bare Application is forbidden.
        Assert.False(refs.Contains("Logistics.Application"),
            $"{assemblyName} directly references Logistics.Application — should reference Logistics.Application.Abstractions only.");
    }

    private static IEnumerable<string> ReferencedAssemblyNames(Assembly asm) =>
        asm.GetReferencedAssemblies()
            .Select(r => r.Name)
            .Where(n => n is not null)
            .Select(n => n!);

    private static string FormatFailures(TestResult r) =>
        r.FailingTypeNames is null
            ? "(no details)"
            : string.Join("\n  ", r.FailingTypeNames);
}

using Xunit;
using Logistics.Application.Modules.Integrations.Webhooks.Commands;

namespace Logistics.Architecture.Tests;

/// <summary>
/// XML-level rules over .csproj files — catches package/project references that
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
    public void Each_Infrastructure_csproj_does_not_reference_Application_project(string projectName)
    {
        var csproj = CsprojAssertions.ResolveRepoFile(
            "src", "Infrastructure", projectName, $"{projectName}.csproj");
        CsprojAssertions.AssertNoProjectReference(csproj, "Logistics.Application.csproj");
    }
}

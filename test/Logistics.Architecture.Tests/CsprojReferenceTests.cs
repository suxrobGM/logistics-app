using Xunit;

namespace Logistics.Architecture.Tests;

/// <summary>Catches references IL inspection can't see, such as a declared but unused package.</summary>
public class CsprojReferenceTests
{
    [Fact]
    public void Application_csproj_has_no_AspNetCore_Http_package()
    {
        var csproj = CsprojAssertions.ResolveRepoFile(
            "src", "Core", "Logistics.Application", "Logistics.Application.csproj");
        CsprojAssertions.AssertNoPackage(csproj, "Microsoft.AspNetCore.Http");
    }

    [Fact]
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

    [Fact]
    public void Abstractions_csproj_has_no_Stripe_package()
    {
        var csproj = CsprojAssertions.ResolveRepoFile(
            "src", "Core", "Logistics.Application.Abstractions", "Logistics.Application.Abstractions.csproj");
        CsprojAssertions.AssertNoPackage(csproj, "Stripe.net");
    }

    /// <summary>Discovered from disk, so a new infrastructure project is covered without registering it.</summary>
    public static TheoryData<string> InfrastructureProjects =>
    [
        .. Directory.EnumerateDirectories(CsprojAssertions.ResolveRepoFile("src", "Infrastructure"))
            .Order()
            .Select(Path.GetFileName)
            .OfType<string>()
            .Where(name => !AssemblyAnchors.BoundaryExempt.Contains(name))
            .Where(name => File.Exists(CsprojAssertions.ResolveRepoFile("src", "Infrastructure", name, $"{name}.csproj")))
    ];

    [Theory]
    [MemberData(nameof(InfrastructureProjects))]
    public void Each_Infrastructure_csproj_does_not_reference_Application_project(string projectName)
    {
        var csproj = CsprojAssertions.ResolveRepoFile(
            "src", "Infrastructure", projectName, $"{projectName}.csproj");
        CsprojAssertions.AssertNoProjectReference(csproj, "Logistics.Application.csproj");
    }
}

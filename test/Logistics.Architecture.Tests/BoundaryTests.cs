using ArchUnitNET.Domain;
using ArchUnitNET.xUnitV3;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;

namespace Logistics.Architecture.Tests;

/// <summary>
/// External SDKs are checked through raw IL dependencies (<see cref="DependsOnNamespace"/>),
/// so their assemblies never have to be loaded into the architecture.
/// </summary>
public class BoundaryTests
{
    [Fact]
    public void Application_must_not_depend_on_AspNetCore_Http()
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.Application)
            .Should().FollowCustomCondition(
                cls => !DependsOnNamespace(cls, "Microsoft.AspNetCore.Http"),
                "not depend on Microsoft.AspNetCore.Http.*",
                "depends on Microsoft.AspNetCore.Http.*")
            .Check(AssemblyAnchors.Architecture);
    }

    [Fact]
    public void Application_must_not_depend_on_Stripe()
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.Application)
            .Should().FollowCustomCondition(
                cls => !DependsOnNamespace(cls, "Stripe"),
                "not depend on Stripe.*",
                "depends on Stripe.*")
            .Check(AssemblyAnchors.Architecture);
    }

    [Fact]
    public void Application_must_not_reference_Infrastructure_assemblies()
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.Application)
            .Should().NotDependOnAny(AssemblyAnchors.InfrastructureTypes)
            .Check(AssemblyAnchors.Architecture);
    }

    [Fact]
    public void Abstractions_must_not_reference_Application_or_Infrastructure_assemblies()
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.ApplicationAbstractions)
            .Should().NotDependOnAny(Types().That().ResideInAssembly(AssemblyAnchors.Application))
            .AndShould().NotDependOnAny(AssemblyAnchors.InfrastructureTypes)
            .Check(AssemblyAnchors.Architecture);
    }

    [Fact]
    public void Abstractions_must_not_depend_on_EFCore_or_AspNetCore_Http()
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.ApplicationAbstractions)
            .Should().FollowCustomCondition(
                cls => !DependsOnNamespace(cls, "Microsoft.EntityFrameworkCore")
                       && !DependsOnNamespace(cls, "Microsoft.AspNetCore.Http"),
                "not depend on Microsoft.EntityFrameworkCore.* or Microsoft.AspNetCore.Http.*",
                "depends on Microsoft.EntityFrameworkCore.* or Microsoft.AspNetCore.Http.*")
            .Check(AssemblyAnchors.Architecture);
    }

    [Fact]
    public void Abstractions_must_not_depend_on_Stripe()
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.ApplicationAbstractions)
            .Should().FollowCustomCondition(
                cls => !DependsOnNamespace(cls, "Stripe"),
                "not depend on Stripe.*",
                "depends on Stripe.*")
            .Check(AssemblyAnchors.Architecture);
    }

    public static TheoryData<string> InfrastructureAssemblies =>
    [
        .. AssemblyAnchors.AllInfrastructure
            .Select(a => a.GetName().Name!)
            .Where(n => !AssemblyAnchors.BoundaryExempt.Contains(n))
            .Order()
    ];

    [Theory]
    [MemberData(nameof(InfrastructureAssemblies))]
    public void Each_Infrastructure_assembly_references_Abstractions_not_Application(string assemblyName)
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.ByName(assemblyName))
            .Should().NotDependOnAny(Types().That().ResideInAssembly(AssemblyAnchors.Application))
            .Check(AssemblyAnchors.Architecture);
    }

    /// <summary>Mediator sits below Domain, so it is the leaf of the graph.</summary>
    [Fact]
    public void Mediator_must_not_depend_on_any_other_Logistics_assembly()
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.Mediator)
            .Should().FollowCustomCondition(
                cls => !cls.Dependencies.Any(d =>
                    d.Target.FullName is { } name
                    && name.StartsWith("Logistics.", StringComparison.Ordinal)
                    && !name.StartsWith("Logistics.Mediator", StringComparison.Ordinal)),
                "not depend on any other Logistics.* assembly",
                "depends on another Logistics.* assembly")
            .Check(AssemblyAnchors.Architecture);
    }

    private static bool DependsOnNamespace(IType cls, string namespacePrefix) =>
        cls.Dependencies.Any(d =>
            d.Target.FullName is { } fullName
            && (fullName.StartsWith(namespacePrefix + ".", StringComparison.Ordinal)
                || fullName.Equals(namespacePrefix, StringComparison.Ordinal)));
}

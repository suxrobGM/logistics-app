using ArchUnitNET.xUnit;
using Xunit;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using Logistics.Application.Modules.Integrations.Webhooks.Commands;

namespace Logistics.Architecture.Tests;

/// <summary>
/// Layering rules over compiled assemblies, expressed in ArchUnitNET.
///
/// Internal cross-assembly checks use <c>ResideInAssembly(Assembly)</c>, which compares
/// by Assembly identity — so <c>Logistics.Application</c> and
/// <c>Logistics.Application.Abstractions</c> are cleanly separated (unlike NetArchTest's
/// prefix matching, which forced us into reflection workarounds).
///
/// External SDK checks (Stripe, AspNetCore.Http, EFCore) use raw IL-level dependency
/// inspection (<c>cls.Dependencies</c>) via <c>FollowCustomCondition</c>, which works
/// without having to load those framework assemblies into the Architecture.
///
/// Some assertions are <c>[Fact(Skip = ...)]</c> because slice 1.8 (Stripe SDK→DTO replacement)
/// and slice 1.9-AI (Infrastructure.AI decoupling) are deferred. Re-enable when those land.
/// See <c>.claude/plans/refactor-application-layer/REFACTOR-INDEX.md</c>.
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

    [Fact(Skip = "Re-enable when slice 1.8 lands; ProcessStripEventHandler still consumes raw Stripe.Event payloads.")]
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
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(
                    AssemblyAnchors.InfrastructureAI,
                    AssemblyAnchors.InfrastructureCommunications,
                    AssemblyAnchors.InfrastructureDocuments,
                    AssemblyAnchors.InfrastructureEld,
                    AssemblyAnchors.InfrastructureLoadBoard,
                    AssemblyAnchors.InfrastructurePayments,
                    AssemblyAnchors.InfrastructurePersistence,
                    AssemblyAnchors.InfrastructureRouting,
                    AssemblyAnchors.InfrastructureStorage,
                    AssemblyAnchors.InfrastructureTax,
                    AssemblyAnchors.InfrastructureVin))
            .Check(AssemblyAnchors.Architecture);
    }

    [Fact]
    public void Abstractions_must_not_reference_Application_or_Infrastructure_assemblies()
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.ApplicationAbstractions)
            .Should().NotDependOnAny(
                Types().That().ResideInAssembly(
                    AssemblyAnchors.Application,
                    AssemblyAnchors.InfrastructureAI,
                    AssemblyAnchors.InfrastructureCommunications,
                    AssemblyAnchors.InfrastructureDocuments,
                    AssemblyAnchors.InfrastructureEld,
                    AssemblyAnchors.InfrastructureLoadBoard,
                    AssemblyAnchors.InfrastructurePayments,
                    AssemblyAnchors.InfrastructurePersistence,
                    AssemblyAnchors.InfrastructureRouting,
                    AssemblyAnchors.InfrastructureStorage,
                    AssemblyAnchors.InfrastructureTax,
                    AssemblyAnchors.InfrastructureVin))
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

    [Fact(Skip = "Re-enable when slice 1.8 lands; Abstractions still ships Stripe.net to support port signatures that take Stripe SDK types.")]
    public void Abstractions_must_not_depend_on_Stripe()
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.ApplicationAbstractions)
            .Should().FollowCustomCondition(
                cls => !DependsOnNamespace(cls, "Stripe"),
                "not depend on Stripe.*",
                "depends on Stripe.*")
            .Check(AssemblyAnchors.Architecture);
    }

    /// <summary>
    /// Derived from AssemblyAnchors.AllInfrastructure, so a new infrastructure project is covered
    /// by this rule as soon as it has an anchor — no separate list to remember here.
    /// </summary>
    public static TheoryData<string> InfrastructureAssemblies
    {
        get
        {
            var data = new TheoryData<string>();
            var names = AssemblyAnchors.AllInfrastructure
                .Select(a => a.GetName().Name!)
                .Where(n => !AssemblyAnchors.BoundaryExempt.Contains(n))
                .Order();

            foreach (var name in names)
            {
                data.Add(name);
            }

            return data;
        }
    }

    [Theory]
    [MemberData(nameof(InfrastructureAssemblies))]
    public void Each_Infrastructure_assembly_references_Abstractions_not_Application(string assemblyName)
    {
        Classes().That().ResideInAssembly(AssemblyAnchors.ByName(assemblyName))
            .Should().NotDependOnAny(Types().That().ResideInAssembly(AssemblyAnchors.Application))
            .Check(AssemblyAnchors.Architecture);
    }

    private static bool DependsOnNamespace(ArchUnitNET.Domain.IType cls, string namespacePrefix) =>
        cls.Dependencies.Any(d =>
            d.Target.FullName is { } fullName &&
            (fullName.StartsWith(namespacePrefix + ".", StringComparison.Ordinal)
             || fullName.Equals(namespacePrefix, StringComparison.Ordinal)));
}

using ArchUnitNET.Domain;
using ArchUnitNET.Fluent;
using ArchUnitNET.Loader;
using Logistics.Application.Abstractions.CurrentUser;
using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchitectureModel = ArchUnitNET.Domain.Architecture;
using Assembly = System.Reflection.Assembly;

namespace Logistics.Architecture.Tests;

/// <summary>
/// One public type per assembly under test, which loads it for <see cref="Architecture"/>.
/// A new infrastructure project joins <see cref="AllInfrastructure"/>; a new <c>src/Core</c>
/// project needs an anchor, a place in <see cref="Architecture"/>, and its own rule.
/// Miss either and nothing checks the project.
/// </summary>
internal static class AssemblyAnchors
{
    public static readonly Assembly Application = typeof(Logistics.Application.Registrar).Assembly;
    public static readonly Assembly ApplicationAbstractions = typeof(ICurrentUserService).Assembly;
    public static readonly Assembly Domain = typeof(Logistics.Domain.Entities.Tenant).Assembly;
    public static readonly Assembly Mediator = typeof(global::Logistics.Mediator.IMediator).Assembly;
    public static readonly Assembly SharedModels = typeof(Logistics.Shared.Models.PagedQuery).Assembly;

    public static readonly Assembly[] AllInfrastructure =
    [
        typeof(Logistics.Infrastructure.AI.Registrar).Assembly,
        typeof(Logistics.Infrastructure.Communications.Registrar).Assembly,
        typeof(Logistics.Infrastructure.Documents.Registrar).Assembly,
        typeof(Logistics.Infrastructure.Integrations.Registrar).Assembly,
        typeof(Logistics.Infrastructure.Payments.Registrar).Assembly,
        typeof(Logistics.Infrastructure.Persistence.Registrar).Assembly,
        typeof(Logistics.Infrastructure.Routing.Registrar).Assembly,
        typeof(Logistics.Infrastructure.Storage.Registrar).Assembly,
    ];

    /// <summary>Exempt from "must not depend on Application" until Infrastructure.AI is decoupled.</summary>
    public static readonly string[] BoundaryExempt = ["Logistics.Infrastructure.AI"];

    public static IObjectProvider<IType> InfrastructureTypes =>
        Types().That().ResideInAssembly(AllInfrastructure[0], AllInfrastructure[1..]);

    public static Assembly ByName(string name) =>
        AllInfrastructure.FirstOrDefault(a => a.GetName().Name == name)
        ?? throw new InvalidOperationException($"No infrastructure anchor for assembly '{name}'.");

    public static readonly ArchitectureModel Architecture = new ArchLoader()
        .LoadAssemblies([Application, ApplicationAbstractions, Domain, Mediator, SharedModels, .. AllInfrastructure])
        .Build();
}

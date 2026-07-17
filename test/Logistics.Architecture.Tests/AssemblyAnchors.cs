// Force every project under test to be loaded into the AppDomain and exposes
// the ArchUnitNET Architecture used across all boundary/handler tests.
// Each anchor references a real public type from the target assembly's Registrar
// (touching them is enough to pull the assembly in).
//
// Adding a new infrastructure project? Add its anchor to AllInfrastructure below — the
// ArchLoader architecture and the boundary Theory both derive from that one array.

using ArchUnitNET.Loader;
using Logistics.Application.Abstractions.CurrentUser;
using Assembly = System.Reflection.Assembly;
using ArchitectureModel = ArchUnitNET.Domain.Architecture;

namespace Logistics.Architecture.Tests;

internal static class AssemblyAnchors
{
    public static readonly Assembly Application = typeof(Logistics.Application.Registrar).Assembly;
    public static readonly Assembly ApplicationAbstractions = typeof(ICurrentUserService).Assembly;
    public static readonly Assembly Domain = typeof(Logistics.Domain.Entities.Tenant).Assembly;

    public static readonly Assembly InfrastructureAI = typeof(Logistics.Infrastructure.AI.Registrar).Assembly;
    public static readonly Assembly InfrastructureAccounting = typeof(Logistics.Infrastructure.Integrations.Accounting.Registrar).Assembly;
    public static readonly Assembly InfrastructureIntegrationsCommon = typeof(Logistics.Infrastructure.Integrations.Common.HttpClientJsonExtensions).Assembly;
    public static readonly Assembly InfrastructureCommunications = typeof(Logistics.Infrastructure.Communications.Registrar).Assembly;
    public static readonly Assembly InfrastructureDocuments = typeof(Logistics.Infrastructure.Documents.Registrar).Assembly;
    public static readonly Assembly InfrastructureEld = typeof(Logistics.Infrastructure.Integrations.Eld.Registrar).Assembly;
    public static readonly Assembly InfrastructureFuelCards = typeof(Logistics.Infrastructure.Integrations.FuelCards.Registrar).Assembly;
    public static readonly Assembly InfrastructureLoadBoard = typeof(Logistics.Infrastructure.Integrations.LoadBoard.Registrar).Assembly;
    public static readonly Assembly InfrastructurePayments = typeof(Logistics.Infrastructure.Payments.Registrar).Assembly;
    public static readonly Assembly InfrastructurePersistence = typeof(Logistics.Infrastructure.Persistence.Registrar).Assembly;
    public static readonly Assembly InfrastructureRouting = typeof(Logistics.Infrastructure.Routing.Registrar).Assembly;
    public static readonly Assembly InfrastructureStorage = typeof(Logistics.Infrastructure.Storage.Registrar).Assembly;
    public static readonly Assembly InfrastructureTax = typeof(Logistics.Infrastructure.Tax.Registrar).Assembly;
    public static readonly Assembly InfrastructureVin = typeof(Logistics.Infrastructure.Vin.Registrar).Assembly;

    public static readonly Assembly[] AllInfrastructure =
    [
        InfrastructureAI,
        InfrastructureAccounting,
        InfrastructureIntegrationsCommon,
        InfrastructureCommunications,
        InfrastructureDocuments,
        InfrastructureEld,
        InfrastructureFuelCards,
        InfrastructureLoadBoard,
        InfrastructurePayments,
        InfrastructurePersistence,
        InfrastructureRouting,
        InfrastructureStorage,
        InfrastructureTax,
        InfrastructureVin,
    ];

    /// <summary>
    /// Infrastructure assemblies exempt from the "must not depend on Application" boundary rule.
    /// Logistics.Infrastructure.AI stays here until slice 1.9-AI decouples it. See REFACTOR-INDEX.md.
    /// </summary>
    public static readonly string[] BoundaryExempt = ["Logistics.Infrastructure.AI"];

    public static Assembly ByName(string name) =>
        AllInfrastructure.FirstOrDefault(a => a.GetName().Name == name)
        ?? throw new InvalidOperationException($"No infrastructure anchor for assembly '{name}'.");

    public static readonly ArchitectureModel Architecture = new ArchLoader()
        .LoadAssemblies([Application, ApplicationAbstractions, Domain, .. AllInfrastructure])
        .Build();
}

// Force every project under test to be loaded into the AppDomain and exposes
// the ArchUnitNET Architecture used across all boundary/handler tests.
// Each anchor references a real public type from the target assembly's Registrar
// (touching them is enough to pull the assembly in).

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
    public static readonly Assembly InfrastructureCommunications = typeof(Logistics.Infrastructure.Communications.Registrar).Assembly;
    public static readonly Assembly InfrastructureDocuments = typeof(Logistics.Infrastructure.Documents.Registrar).Assembly;
    public static readonly Assembly InfrastructureEld = typeof(Logistics.Infrastructure.Integrations.Eld.Registrar).Assembly;
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
        InfrastructureCommunications,
        InfrastructureDocuments,
        InfrastructureEld,
        InfrastructureLoadBoard,
        InfrastructurePayments,
        InfrastructurePersistence,
        InfrastructureRouting,
        InfrastructureStorage,
        InfrastructureTax,
        InfrastructureVin,
    ];

    public static readonly ArchitectureModel Architecture = new ArchLoader()
        .LoadAssemblies(
            Application,
            ApplicationAbstractions,
            Domain,
            InfrastructureAI,
            InfrastructureCommunications,
            InfrastructureDocuments,
            InfrastructureEld,
            InfrastructureLoadBoard,
            InfrastructurePayments,
            InfrastructurePersistence,
            InfrastructureRouting,
            InfrastructureStorage,
            InfrastructureTax,
            InfrastructureVin)
        .Build();
}

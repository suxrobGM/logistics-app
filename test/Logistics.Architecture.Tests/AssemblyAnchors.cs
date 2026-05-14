// Force every project under test to be loaded into the AppDomain so NetArchTest's
// Types.InAssembly(...) can resolve them. Each entry references a real public type
// from the corresponding assembly's Registrar — touching them is enough.

using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Architecture.Tests;

internal static class AssemblyAnchors
{
    public static readonly Type Application = typeof(Logistics.Application.Registrar);
    public static readonly Type ApplicationAbstractions = typeof(ICurrentUserService);
    public static readonly Type Domain = typeof(Logistics.Domain.Entities.Tenant);

    public static readonly Type InfrastructureAI = typeof(Logistics.Infrastructure.AI.Registrar);
    public static readonly Type InfrastructureCommunications = typeof(Logistics.Infrastructure.Communications.Registrar);
    public static readonly Type InfrastructureDocuments = typeof(Logistics.Infrastructure.Documents.Registrar);
    public static readonly Type InfrastructureEld = typeof(Logistics.Infrastructure.Integrations.Eld.Registrar);
    public static readonly Type InfrastructureLoadBoard = typeof(Logistics.Infrastructure.Integrations.LoadBoard.Registrar);
    public static readonly Type InfrastructurePayments = typeof(Logistics.Infrastructure.Payments.Registrar);
    public static readonly Type InfrastructurePersistence = typeof(Logistics.Infrastructure.Persistence.Registrar);
    public static readonly Type InfrastructureRouting = typeof(Logistics.Infrastructure.Routing.Registrar);
    public static readonly Type InfrastructureStorage = typeof(Logistics.Infrastructure.Storage.Registrar);
    public static readonly Type InfrastructureTax = typeof(Logistics.Infrastructure.Tax.Registrar);
    public static readonly Type InfrastructureVin = typeof(Logistics.Infrastructure.Vin.Registrar);

    public static System.Reflection.Assembly LoadByName(string assemblyName)
    {
        // Force-touch all anchors so the assemblies are loaded into AppDomain.
        _ = Application; _ = ApplicationAbstractions; _ = Domain;
        _ = InfrastructureAI; _ = InfrastructureCommunications; _ = InfrastructureDocuments;
        _ = InfrastructureEld; _ = InfrastructureLoadBoard; _ = InfrastructurePayments;
        _ = InfrastructurePersistence; _ = InfrastructureRouting; _ = InfrastructureStorage;
        _ = InfrastructureTax; _ = InfrastructureVin;

        var asm = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(a => a.GetName().Name == assemblyName);

        if (asm is null)
        {
            throw new InvalidOperationException(
                $"Assembly '{assemblyName}' not loaded. Check Logistics.Architecture.Tests.csproj has a ProjectReference to it.");
        }

        return asm;
    }
}

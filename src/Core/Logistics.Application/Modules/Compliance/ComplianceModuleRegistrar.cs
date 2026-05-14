using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.Compliance;

public static class ComplianceModuleRegistrar
{
    /// <summary>
    /// Registers Compliance-module-specific services that cannot be covered by Scrutor's
    /// <see cref="IApplicationService"/> scan (decorators, named instances, factories).
    /// Empty stub today; populated in Phase 7 when files move under Modules/Compliance/.
    /// </summary>
    public static IServiceCollection AddComplianceModule(this IServiceCollection services)
    {
        return services;
    }
}

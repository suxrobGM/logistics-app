using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.Integrations;

public static class IntegrationsModuleRegistrar
{
    /// <summary>
    /// Registers Integrations-module-specific services that cannot be covered by Scrutor's
    /// <see cref="IApplicationService"/> scan (decorators, named instances, factories).
    /// Empty stub today; populated in Phase 7 when files move under Modules/Integrations/.
    /// </summary>
    public static IServiceCollection AddIntegrationsModule(this IServiceCollection services)
    {
        return services;
    }
}

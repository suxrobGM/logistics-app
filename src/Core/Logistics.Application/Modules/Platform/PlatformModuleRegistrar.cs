using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.Platform;

public static class PlatformModuleRegistrar
{
    /// <summary>
    /// Registers Platform-module-specific services that cannot be covered by Scrutor's
    /// <see cref="IApplicationService"/> scan (decorators, named instances, factories).
    /// Empty stub today; populated in Phase 7 when files move under Modules/Platform/.
    /// </summary>
    public static IServiceCollection AddPlatformModule(this IServiceCollection services)
    {
        return services;
    }
}

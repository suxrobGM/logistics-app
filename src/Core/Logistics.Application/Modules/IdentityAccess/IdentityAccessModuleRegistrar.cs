using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.IdentityAccess;

public static class IdentityAccessModuleRegistrar
{
    /// <summary>
    /// Registers IdentityAccess-module-specific services that cannot be covered by Scrutor's
    /// <see cref="IApplicationService"/> scan (decorators, named instances, factories).
    /// Empty stub today; populated in Phase 7 when files move under Modules/IdentityAccess/.
    /// </summary>
    public static IServiceCollection AddIdentityAccessModule(this IServiceCollection services)
    {
        return services;
    }
}

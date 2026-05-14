using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.Operations;

public static class OperationsModuleRegistrar
{
    /// <summary>
    /// Registers Operations-module-specific services that cannot be covered by Scrutor's
    /// <see cref="IApplicationService"/> scan (decorators, named instances, factories).
    /// Empty stub today; populated in Phase 7 when files move under Modules/Operations/.
    /// </summary>
    public static IServiceCollection AddOperationsModule(this IServiceCollection services)
    {
        return services;
    }
}

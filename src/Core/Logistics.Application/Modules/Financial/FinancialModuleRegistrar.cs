using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.Financial;

public static class FinancialModuleRegistrar
{
    /// <summary>
    /// Registers Financial-module-specific services that cannot be covered by Scrutor's
    /// <see cref="IApplicationService"/> scan (decorators, named instances, factories).
    /// Empty stub today; populated in Phase 7 when files move under Modules/Financial/.
    /// </summary>
    public static IServiceCollection AddFinancialModule(this IServiceCollection services)
    {
        return services;
    }
}

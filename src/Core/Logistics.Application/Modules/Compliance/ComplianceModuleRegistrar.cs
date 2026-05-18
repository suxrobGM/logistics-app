using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.Compliance;

public static class ComplianceModuleRegistrar
{
    /// <summary>
    /// Registers Compliance-module-specific services that cannot be covered by Scrutor's
    /// <see cref="IApplicationService"/> scan (decorators, named instances, factories).
    /// MediatR handlers + FluentValidation + IApplicationService scan happen at the assembly
    /// level in <c>Registrar.AddApplicationCommon</c>/<c>AddApplicationServices</c>.
    /// </summary>
    public static IServiceCollection AddComplianceModule(this IServiceCollection services)
    {
        return services;
    }
}

using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.Compliance;

public static class ComplianceModuleRegistrar
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers Compliance-module-specific services that cannot be covered by Scrutor's
        /// <see cref="IApplicationService"/> scan (decorators, named instances, factories).
        /// Handlers + FluentValidation + IApplicationService scan happen at the assembly
        /// level in <c>Registrar.AddApplicationCommon</c>/<c>AddApplicationServices</c>.
        /// </summary>
        public IServiceCollection AddComplianceModule()
        {
            return services;
        }
    }
}

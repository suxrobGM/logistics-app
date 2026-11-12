using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.IdentityAccess;

public static class IdentityAccessModuleRegistrar
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers IdentityAccess-module-specific services that cannot be covered by Scrutor's
        /// <see cref="IApplicationService"/> scan (decorators, named instances, factories).
        /// Handlers + FluentValidation + IApplicationService scan happen at the assembly
        /// level in <c>Registrar.AddApplicationCommon</c>/<c>AddApplicationServices</c>.
        /// </summary>
        public IServiceCollection AddIdentityAccessModule()
        {
            return services;
        }
    }
}

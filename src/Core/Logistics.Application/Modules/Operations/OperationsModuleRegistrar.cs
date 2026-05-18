using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.Operations;

public static class OperationsModuleRegistrar
{
    /// <summary>
    /// Registers Operations-module-specific services that cannot be covered by Scrutor's
    /// <see cref="IApplicationService"/> scan (decorators, named instances, factories).
    /// MediatR handlers + FluentValidation + IApplicationService scan happen at the assembly
    /// level in <c>Registrar.AddApplicationCommon</c>/<c>AddApplicationServices</c>.
    /// </summary>
    public static IServiceCollection AddOperationsModule(this IServiceCollection services)
    {
        return services;
    }
}

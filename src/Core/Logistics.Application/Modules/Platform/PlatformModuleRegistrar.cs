using Logistics.Application.Modules.Platform.ProductLicense.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Application.Modules.Platform;

public static class PlatformModuleRegistrar
{
    /// <summary>
    /// Registers Platform-module-specific services that cannot be covered by Scrutor's
    /// <see cref="IApplicationService"/> scan (decorators, named instances, factories).
    /// MediatR handlers + FluentValidation + IApplicationService scan happen at the assembly
    /// level in <c>Registrar.AddApplicationCommon</c>/<c>AddApplicationServices</c>.
    /// </summary>
    public static IServiceCollection AddPlatformModule(this IServiceCollection services)
    {
        // Singleton: importing the public key once is enough, and the validator holds no state.
        services.AddSingleton(_ => new ProductLicenseKeyValidator(ProductLicensePublicKey.SpkiBase64));
        return services;
    }
}

using Logistics.Application.Abstractions.ProductLicense;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Licensing;

public static class Registrar
{
    /// <summary>
    ///     Registers the outbound license heartbeat sender. The receiver lives in the API.
    /// </summary>
    public static IServiceCollection AddLicensingInfrastructure(this IServiceCollection services)
    {
        services.AddHttpClient<IProductLicenseHeartbeatSender, ProductLicenseHeartbeatSender>(
            client => client.Timeout = TimeSpan.FromSeconds(15));
        return services;
    }
}

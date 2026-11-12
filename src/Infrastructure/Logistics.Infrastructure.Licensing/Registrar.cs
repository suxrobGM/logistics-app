using Logistics.Application.Abstractions.ProductLicense;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Licensing;

public static class Registrar
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers the outbound license heartbeat sender. The receiver lives in the API.
        /// </summary>
        public IServiceCollection AddLicensingInfrastructure()
        {
            services.AddHttpClient<IProductLicenseHeartbeatSender, ProductLicenseHeartbeatSender>(
                client => client.Timeout = TimeSpan.FromSeconds(15));
            return services;
        }
    }
}

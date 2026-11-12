using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Abstractions.Vin;

namespace Logistics.Infrastructure.Vin;

public static class Registrar
{
    extension(IServiceCollection services)
    {
        /// <summary>
        ///     Registers the VIN decoder chain: WMI prefix lookup + NHTSA full-VIN decode,
        ///     fronted by <see cref="CompositeVinDecoderService"/>.
        /// </summary>
        public IServiceCollection AddVinInfrastructure()
        {
            services.AddHttpClient<WmiPrefixDecoder>();
            services.AddHttpClient<NhtsaVinDecoderService>();
            services.AddTransient<IVinDecoderService, CompositeVinDecoderService>();
            return services;
        }
    }
}

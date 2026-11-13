using Logistics.Application.Abstractions.Routing;
using Logistics.Infrastructure.Routing.Geocoding;
using Logistics.Infrastructure.Routing.Geospatial;
using Logistics.Infrastructure.Routing.Optimization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Routing;

public static class Registrar
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddRoutingInfrastructure(IConfiguration configuration)
        {
            // Geocoding services
            services.Configure<MapboxOptions>(configuration.GetSection(MapboxOptions.SectionName));
            services.AddHttpClient<IGeocodingService, MapboxGeocodingService>();
            services.AddHttpClient<MapboxMatrixClient>();

            // Offline point-in-polygon jurisdiction lookup (IFTA mileage attribution)
            services.AddSingleton<IJurisdictionResolver, JurisdictionResolver>();
            services.AddHostedService<JurisdictionResolverWarmup>();

            // Trip optimization services
            services.AddSingleton<HeuristicTripOptimizer>();
            services.AddSingleton<MapboxMatrixTripOptimizer>();
            services.AddSingleton<ITripOptimizer, CompositeTripOptimizer>();
            services.AddTransient<IRouteDurationService, MapboxRouteDurationService>();
            return services;
        }
    }
}

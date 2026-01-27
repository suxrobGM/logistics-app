using Logistics.Application.Services;
using Logistics.Application.Services.Geocoding;
using Logistics.Infrastructure.Routing.Geocoding;
using Logistics.Infrastructure.Routing.Optimization;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Routing;

public static class Registrar
{
    public static IServiceCollection AddRoutingInfrastructure(this IServiceCollection services)
    {
        // Geocoding services
        services.AddHttpClient<IGeocodingService, MapboxGeocodingService>();
        services.AddHttpClient<MapboxMatrixClient>();

        // Trip optimization services
        services.AddSingleton<HeuristicTripOptimizer>();
        services.AddSingleton<MapboxMatrixTripOptimizer>();
        services.AddSingleton<ITripOptimizer, CompositeTripOptimizer>();
        return services;
    }
}

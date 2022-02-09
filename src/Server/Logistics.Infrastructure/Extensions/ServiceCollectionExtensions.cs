using Logistics.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        
        
        services.AddScoped<ICargoRepository, CargoRepository>();
        services.AddScoped<ITruckRepository, TruckRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
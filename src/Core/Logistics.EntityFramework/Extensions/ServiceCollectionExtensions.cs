using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Domain.Repositories;
using Logistics.EntityFramework.Data;
using Logistics.EntityFramework.Repositories;

namespace Logistics.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkLayer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Local");

        services.AddDbContext<DatabaseContext>(options =>
        {
            options.UseSqlServer(connectionString)
                .UseLazyLoadingProxies();
        });
        services.AddScoped<ICargoRepository, CargoRepository>();
        services.AddScoped<ITruckRepository, TruckRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        return services;
    }
}
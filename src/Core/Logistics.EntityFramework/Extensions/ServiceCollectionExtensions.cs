using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.EntityFramework.Repositories;
using Logistics.EntityFramework.Helpers;

namespace Logistics.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddEntityFrameworkLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "Local")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        services.AddDbContext<DatabaseContext>(
            o => DbContextHelpers.ConfigureMySql(connectionString, o));

        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
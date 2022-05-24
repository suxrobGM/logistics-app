using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.EntityFramework.Repositories;
using Logistics.EntityFramework.Helpers;

namespace Logistics.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "Local")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        services.AddDbContext<TenantDbContext>();
        services.AddDbContext<MainDbContext>(o => DbContextHelpers.ConfigureMySql(connectionString, o));

        services.AddScoped(typeof(IMainRepository<>), typeof(MainRepository<>));
        services.AddScoped(typeof(ITenantRepository<>), typeof(TenantRepository<>));
        services.AddScoped(typeof(IMainUnitOfWork), typeof(MainUnitOfWork));
        services.AddScoped(typeof(ITenantUnitOfWork), typeof(TenantUnitOfWork));
        return services;
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Domain.Options;
using Logistics.Domain.Services;
using Logistics.EntityFramework.Repositories;
using Logistics.EntityFramework.Helpers;
using Logistics.EntityFramework.Services;

namespace Logistics.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "Local",
        string tenantsConfigSection = "TenantsConfig")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);
        var tenantsSettings = configuration.GetSection(tenantsConfigSection).Get<TenantsSettings>();

        if (tenantsSettings.DatabaseProvider == "mysql")
        {
            services.AddScoped<IDatabaseProviderService, MySqlProviderService>();
        }

        services.AddDbContext<TenantDbContext>();
        services.AddDbContext<MainDbContext>(o => DbContextHelpers.ConfigureMySql(connectionString, o));

        services.AddIdentityCore<User>()
            .AddEntityFrameworkStores<MainDbContext>();

        services.AddSingleton(tenantsSettings);
        services.AddScoped(typeof(ITenantService), typeof(TenantService));
        services.AddScoped(typeof(IMainRepository<>), typeof(MainRepository<>));
        services.AddScoped(typeof(ITenantRepository<>), typeof(TenantRepository<>));
        services.AddScoped(typeof(IMainUnitOfWork), typeof(MainUnitOfWork));
        services.AddScoped(typeof(ITenantUnitOfWork), typeof(TenantUnitOfWork));
        return services;
    }
}
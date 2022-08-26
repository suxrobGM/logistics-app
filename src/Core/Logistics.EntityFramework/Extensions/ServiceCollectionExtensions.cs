using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Domain.Options;
using Logistics.Domain.Services;
using Logistics.Domain.Shared;
using Logistics.EntityFramework.Repositories;
using Logistics.EntityFramework.Helpers;
using Logistics.EntityFramework.Services;

namespace Logistics.EntityFramework;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDatabases(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? mainDbOptions = null!,
        Action<DbContextOptionsBuilder>? tenantDbOptions = null!,
        string tenantsConfigSection = "TenantsConfig")
    {
        var tenantsSettings = configuration.GetSection(tenantsConfigSection).Get<TenantsSettings>();

        if (tenantsSettings is { DatabaseProvider: "mysql" })
        {
            services.AddScoped<IDatabaseProviderService, MySqlProviderService>();
            services.AddSingleton(tenantsSettings);
        }
        
        services.AddDbContext<TenantDbContext>(tenantDbOptions);
        services.AddDbContext<MainDbContext>(mainDbOptions);
        return services;
    }
    
    public static IServiceCollection AddIdentity(this IServiceCollection services)
    {
        services.AddIdentityCore<User>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.User.AllowedUserNameCharacters =
                "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM0123456789_.";
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<MainDbContext>();
        return services;
    }
    
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string connectionStringName = "Local",
        string tenantsConfigSection = "TenantsConfig")
    {
        var connectionString = configuration.GetConnectionString(connectionStringName);

        services.AddIdentity();
        services.AddDatabases(configuration,
            o => DbContextHelpers.ConfigureMySql(connectionString, o),
            null,
            tenantsConfigSection);

        services.AddScoped(typeof(ITenantService), typeof(TenantService));
        services.AddScoped(typeof(IMainRepository<>), typeof(MainRepository<>));
        services.AddScoped(typeof(ITenantRepository<>), typeof(TenantRepository<>));
        services.AddScoped(typeof(IMainUnitOfWork), typeof(MainUnitOfWork));
        services.AddScoped(typeof(ITenantUnitOfWork), typeof(TenantUnitOfWork));
        return services;
    }
}
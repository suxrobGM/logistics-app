using Microsoft.AspNetCore.Identity;
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
    
    public static IdentityBuilder AddIdentity(this IServiceCollection services)
    {
        var builder = services.AddIdentityCore<User>(options =>
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
        return builder;
    }
    
    public static IServiceCollection AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string mainDbConnectionSection = "MainDatabase",
        string tenantsConfigSection = "TenantsConfig")
    {
        var connectionString = configuration.GetConnectionString(mainDbConnectionSection);
        
        services.AddIdentity();
        services.AddDatabases(configuration,
            o => DbContextHelpers.ConfigureMySql(connectionString, o),
            null,
            tenantsConfigSection);

        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IMainRepository, MainRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<UnitOfWork<MainDbContext>>();
        services.AddScoped<UnitOfWork<TenantDbContext>>();
        return services;
    }
}
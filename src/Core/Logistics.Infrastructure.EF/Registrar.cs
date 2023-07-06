using Logistics.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Infrastructure.EF.Builder;
using Logistics.Infrastructure.EF.Interceptors;
using Logistics.Infrastructure.EF.Persistence;
using Logistics.Infrastructure.EF.Services;

namespace Logistics.Infrastructure.EF;

public static class Registrar
{
    private static void AddMainDatabase(
        IServiceCollection services,
        IConfiguration configuration,
        string connectionSection)
    {
        var connectionString = configuration.GetConnectionString(connectionSection);
        var options = new MainDbContextOptions
        {
            ConnectionString = connectionString
        };
        
        services.AddSingleton(options);
        services.AddDbContext<MainDbContext>();
    }
    
    private static void AddTenantDatabase(
        IServiceCollection services,
        IConfiguration configuration,
        string connectionSection,
        string tenantsConfigSection)
    {
        var tenantsSettings = configuration.GetSection(tenantsConfigSection).Get<TenantsDatabaseOptions>();
        var connectionString = configuration.GetConnectionString(connectionSection);

        if (tenantsSettings != null)
        {
            services.AddScoped<ITenantDatabaseService, TenantDatabaseService>();
            services.AddSingleton(tenantsSettings);
        }
        
        var options = new TenantDbContextOptions
        {
            ConnectionString = connectionString
        };
        
        services.AddSingleton(options);
        services.AddDbContext<TenantDbContext>();
    }
    
    private static IdentityBuilder AddIdentity(IServiceCollection services)
    {
        var identityBuilder = services.AddIdentityCore<User>(options =>
        {
            options.Password.RequiredLength = 8;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.User.AllowedUserNameCharacters =
                "qwertyuiopasdfghjklzxcvbnmQWERTYUIOPASDFGHJKLZXCVBNM0123456789_.@";
            options.User.RequireUniqueEmail = true;
        })
        .AddRoles<AppRole>()
        .AddEntityFrameworkStores<MainDbContext>();

        return identityBuilder;
    }
    
    public static IInfrastructureBuilder AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string defaultTenantDbConnectionSection = "DefaultTenantDatabase",
        string mainDbConnectionSection = "MainDatabase",
        string tenantsConfigSection = "TenantsDatabaseConfig")
    {
        var identityBuilder = AddIdentity(services);
        AddMainDatabase(services, configuration, mainDbConnectionSection);
        AddTenantDatabase(services, configuration, defaultTenantDbConnectionSection, tenantsConfigSection);

        services.AddDomainLayer();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IMainRepository, MainRepository>();
        services.AddScoped<ITenantRepository, TenantRepository>();
        services.AddScoped<UnitOfWork<MainDbContext>>();
        services.AddScoped<UnitOfWork<TenantDbContext>>();
        return new InfrastructureBuilder(identityBuilder);
    }
}
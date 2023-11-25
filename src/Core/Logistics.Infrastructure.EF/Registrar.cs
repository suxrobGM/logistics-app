using Logistics.Domain;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Infrastructure.EF.Builder;
using Logistics.Infrastructure.EF.Data;
using Logistics.Infrastructure.EF.Interceptors;
using Logistics.Infrastructure.EF.Options;
using Logistics.Infrastructure.EF.Persistence;
using Logistics.Infrastructure.EF.Services;

namespace Logistics.Infrastructure.EF;

public static class Registrar
{
    public static IInfrastructureBuilder AddInfrastructureLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string defaultTenantDbConnectionSection = "DefaultTenantDatabase",
        string masterDbConnectionSection = "MasterDatabase",
        string tenantsConfigSection = "TenantsDatabaseConfig")
    {
        var identityBuilder = AddIdentity(services);
        AddMasterDatabase(services, configuration, masterDbConnectionSection);
        AddTenantDatabase(services, configuration, defaultTenantDbConnectionSection, tenantsConfigSection);

        services.AddDomainLayer();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IMasterUnityOfWork, MasterUnitOfWork>();
        services.AddScoped<ITenantUnityOfWork, TenantUnitOfWork>();
        services.AddScoped<IUserService, UserService>();
        return new InfrastructureBuilder(identityBuilder);
    }
    
    private static void AddMasterDatabase(
        IServiceCollection services,
        IConfiguration configuration,
        string connectionSection)
    {
        var connectionString = configuration.GetConnectionString(connectionSection);
        var options = new MasterDbContextOptions
        {
            ConnectionString = connectionString
        };
        
        services.AddSingleton(options);
        services.AddDbContext<MasterDbContext>();
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
        .AddEntityFrameworkStores<MasterDbContext>();

        return identityBuilder;
    }
}

using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Domain.Services;
using Logistics.Infrastructure.EF.Data;
using Logistics.Infrastructure.EF.Options;
using Logistics.Infrastructure.EF.Persistence;
using Logistics.Infrastructure.EF.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Logistics.Infrastructure.EF.Builder;

internal class InfrastructureBuilder : IInfrastructureBuilder
{
    private readonly IConfiguration _configuration;
    private readonly IServiceCollection _services;
    private ILogger<IInfrastructureBuilder>? _logger;
    
    internal InfrastructureBuilder(IServiceCollection services, IConfiguration configuration)
    {
        _configuration = configuration;
        _services = services;
    }
    
    public IInfrastructureBuilder AddIdentity(Action<IdentityBuilder>? configure = null)
    {
        var identityBuilder = _services.AddIdentityCore<User>(options =>
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
        
        configure?.Invoke(identityBuilder);
        return this;
    }

    public IInfrastructureBuilder AddMasterDatabase(Action<MasterDbContextOptions>? configure = null)
    {
        var options = new MasterDbContextOptions();
        configure?.Invoke(options);

        var connectionString = _configuration.GetConnectionString(options.DbConnectionSection);
        options.ConnectionString = connectionString;
        
        _services.AddSingleton(options);
        _services.AddDbContext<MasterDbContext>();
        _services.AddScoped<IMasterUnityOfWork, MasterUnitOfWork>();
        _logger?.LogInformation("Added master database with connection string: {ConnectionString}", connectionString);
        return this;
    }

    public IInfrastructureBuilder AddTenantDatabase(Action<TenantDbContextOptions>? configure = null)
    {
        var options = new TenantDbContextOptions();
        configure?.Invoke(options);

        var tenantsSettings = _configuration.GetSection(options.TenantsConfigSection).Get<TenantsDatabaseOptions>();
        var connectionString = _configuration.GetConnectionString(options.DefaultTenantDbConnectionSection);

        if (tenantsSettings is not null)
        {
            _services.AddScoped<ITenantDatabaseService, TenantDatabaseService>();
            _services.AddSingleton(tenantsSettings);
            _logger?.LogInformation("Tenants database settings: {Settings}", tenantsSettings);
        }

        options.ConnectionString = connectionString;
        _services.AddDbContext<TenantDbContext>();
        _services.AddScoped<ITenantUnityOfWork, TenantUnitOfWork>();
        _logger?.LogInformation("Added default tenant database with connection string: {ConnectionString}", connectionString);
        return this;
    }
    
    public IInfrastructureBuilder UseLogger(ILogger<IInfrastructureBuilder> logger)
    {
        _logger = logger;
        return this;
    }
}
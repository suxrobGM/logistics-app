using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Persistence.Data;
using Logistics.Infrastructure.Persistence.Options;
using Logistics.Infrastructure.Persistence.Repositories;
using Logistics.Infrastructure.Persistence.Services;
using Logistics.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Logistics.Application.Abstractions.Features;
using Logistics.Application.Abstractions.Modules.Platform.ReadModels;
using Logistics.Application.Abstractions.SystemSettings;
using Logistics.Application.Abstractions.Tenancy;
using Logistics.Application.Abstractions.AiDispatch;
using Logistics.Infrastructure.Persistence.Reads.Platform;

namespace Logistics.Infrastructure.Persistence.Builder;

internal sealed class PersistenceInfrastructureBuilder : IPersistenceInfrastructureBuilder
{
    private readonly IConfiguration configuration;
    private readonly IServiceCollection services;
    private ILogger<IPersistenceInfrastructureBuilder>? logger;

    internal PersistenceInfrastructureBuilder(IServiceCollection services, IConfiguration configuration)
    {
        this.configuration = configuration;
        this.services = services;
    }

    public IPersistenceInfrastructureBuilder AddIdentity(Action<IdentityBuilder>? configure = null)
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

        configure?.Invoke(identityBuilder);
        return this;
    }

    public IPersistenceInfrastructureBuilder UseLogger(ILogger<IPersistenceInfrastructureBuilder> infrastructureLogger)
    {
        logger = infrastructureLogger;
        return this;
    }

    public IPersistenceInfrastructureBuilder AddMasterDatabase(Action<MasterDbContextOptions>? configure = null)
    {
        var options = new MasterDbContextOptions();
        configure?.Invoke(options);

        var connectionString = configuration.GetConnectionString(options.DbConnectionSection);
        options.ConnectionString = connectionString;

        services.AddSingleton(options);
        services.AddDbContext<MasterDbContext>();
        services.AddScoped<IMasterUnitOfWork, MasterUnitOfWork>();
        services.AddScoped(typeof(MasterRepository<,>));
        services.AddScoped<ICurrentTenantAccessor, CurrentTenantAccessor>();
        services.AddScoped<IFeatureService, FeatureService>();
        services.AddScoped<IAiQuotaService, AiQuotaService>();
        services.AddScoped<ISystemSettingService, SystemSettingService>();
        logger?.LogInformation("Added master database with connection string: {ConnectionString}", connectionString);
        return this;
    }

    public IPersistenceInfrastructureBuilder AddTenantDatabase(Action<TenantDbContextOptions>? configure = null)
    {
        var options = new TenantDbContextOptions();
        configure?.Invoke(options);

        var tenantsSettings =
            configuration.GetSection(TenantDatabaseDefaults.SectionName).Get<TenantDatabaseDefaults>();
        var connectionString = configuration.GetConnectionString(options.DefaultTenantDbConnectionSection);

        if (tenantsSettings is not null)
        {
            services.AddScoped<ITenantDatabaseService, TenantDatabaseService>();
            services.AddSingleton(tenantsSettings);
            logger?.LogInformation("Tenants database settings: {Settings}", tenantsSettings);
        }

        options.ConnectionString = connectionString;
        services.AddSingleton(options);
        services.AddDbContext<TenantDbContext>();
        services.AddScoped<ITenantUnitOfWork, TenantUnitOfWork>();
        services.AddScoped(typeof(TenantRepository<,>));
        services.AddScoped<ISafetyReportReader, SafetyReportReader>();
        logger?.LogInformation("Added default tenant database with connection string: {ConnectionString}",
            connectionString);
        return this;
    }
}

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Mappers;
using Logistics.Application.Options;
using Logistics.Application.Services;

namespace Logistics.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMainApplicationLayer(
        this IServiceCollection services,
        IConfiguration configuration,
        string tenantsConfigSection = "TenantsConfig")
    {
        services.AddAutoMapper(o =>
        {
            o.AddProfile<TenantProfile>();
        });

        var tenantsSettings = configuration.GetSection(tenantsConfigSection).Get<TenantsSettings>();

        services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);
        services.AddSingleton(tenantsSettings);

        if (tenantsSettings.DatabaseProvider == "mysql")
        {
            services.AddScoped<IDatabaseProviderService, MySqlProviderService>();
        }
        
        return services;
    }
}
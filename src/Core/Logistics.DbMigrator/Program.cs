using Microsoft.EntityFrameworkCore;
using Logistics.DbMigrator;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Services;
using Logistics.EntityFramework.Data;
using Logistics.EntityFramework.Services;
using Microsoft.AspNetCore.Identity;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(configuration =>
    {
        var path = Path.Combine(AppContext.BaseDirectory, "secrets.json");
        configuration.AddJsonFile(path, true);
    })
    .ConfigureServices((ctx, services) =>
    {
        var mainDbConnection = ctx.Configuration.GetConnectionString("LocalMainDatabase");
        var tenantDbConnection = ctx.Configuration.GetConnectionString("LocalDefaultTenantDatabase");
        var tenantsSettings = ctx.Configuration.GetSection("TenantsConfig").Get<TenantsSettings>();
        
        if (tenantsSettings is { DatabaseProvider: "mysql" })
        {
            services.AddScoped<IDatabaseProviderService, MySqlProviderService>();
            services.AddSingleton(tenantsSettings);
        }

        services.AddDbContext<MainDbContext>(o => ConfigureMySql(mainDbConnection, o));
        services.AddDbContext<TenantDbContext>(o => ConfigureMySql(tenantDbConnection, o));

        services.AddIdentityCore<User>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<MainDbContext>();
        services.AddHostedService<SeedDataService>();
    })
    .Build();

await host.RunAsync();


void ConfigureMySql(string connectionString, DbContextOptionsBuilder options)
{
    options.UseMySql(connectionString,
            ServerVersion.AutoDetect(connectionString),
            o =>
            {
                o.EnableRetryOnFailure(8, TimeSpan.FromSeconds(15), null);
                o.EnableStringComparisonTranslations();
            })
        .UseLazyLoadingProxies();
}

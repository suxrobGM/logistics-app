using Microsoft.EntityFrameworkCore;
using Logistics.EntityFramework;
using Logistics.DbMigrator;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(configuration =>
    {
        var path = Path.Combine(AppContext.BaseDirectory, "secrets.json");
        configuration.AddJsonFile(path, true);
    })
    .ConfigureServices((ctx, services) =>
    {
        var mainDbConnection = ctx.Configuration.GetConnectionString("MainDatabase");
        var tenantDbConnection = ctx.Configuration.GetConnectionString("DefaultTenantDatabase");

        services.AddDatabases(ctx.Configuration,
            o => ConfigureMySql(mainDbConnection, o),
            o => ConfigureMySql(tenantDbConnection, o));
        services.AddIdentity();
        
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

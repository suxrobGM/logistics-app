using Microsoft.EntityFrameworkCore;
using Logistics.DbMigrator;
using Logistics.Domain.Entities;
using Logistics.EntityFramework.Data;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(configuration =>
    {
        var path = Path.Combine(AppContext.BaseDirectory, "secrets.json");
        configuration.AddJsonFile(path, true);
    })
    .ConfigureServices((ctx, services) =>
    {
        var mainDbConntection = ctx.Configuration.GetConnectionString("LocalMainDatabase");
        var tenantDbConntection = ctx.Configuration.GetConnectionString("LocalDefaultTenantDatabase");

        services.AddDbContext<MainDbContext>(o => ConfigureMySql(mainDbConntection, o));
        services.AddDbContext<TenantDbContext>(o => ConfigureMySql(tenantDbConntection, o));

        services.AddIdentityCore<User>()
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
                o.EnableStringComparisonTranslations(true);
            })
        .UseLazyLoadingProxies();
}

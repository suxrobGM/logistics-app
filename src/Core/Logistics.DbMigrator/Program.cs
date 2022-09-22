using Logistics.DbMigrator.Services;
using Microsoft.AspNetCore.Http;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(configuration =>
    {
        var secretsFile = Path.Combine(AppContext.BaseDirectory, "secrets.json");
        var testDataFile = Path.Combine(AppContext.BaseDirectory, "testData.json");
        configuration.AddJsonFile(secretsFile, true);
        configuration.AddJsonFile(testDataFile, true);
    })
    .ConfigureServices((ctx, services) =>
    {
        var mainDbConnection = ctx.Configuration.GetConnectionString("MainDatabase");
        var tenantDbConnection = ctx.Configuration.GetConnectionString("DefaultTenantDatabase");

        services.AddInfrastructureLayer(ctx.Configuration)
            .ConfigureMainDatabase(options =>
            {
                options.ConnectionString = mainDbConnection;
            })
            .ConfigureTenantDatabase(options =>
            {
                options.ConnectionString = tenantDbConnection;
                options.UseTenantService = false;
            });

        services.AddHostedService<SeedData>();
        //services.AddHostedService<PopulateData>();
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

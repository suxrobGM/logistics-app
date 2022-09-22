using Logistics.DbMigrator.Services;

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
        services.AddInfrastructureLayer(ctx.Configuration);
        services.AddHostedService<SeedDataService>();
    })
    .Build();

await host.RunAsync();

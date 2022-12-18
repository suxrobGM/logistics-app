using Logistics.DbMigrator.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration(configuration =>
    {
        var secretsFile = Path.Combine(AppContext.BaseDirectory, "appsettings.secrets.json");
        var testDataFile = Path.Combine(AppContext.BaseDirectory, "fakeDataset.json");
        configuration.AddJsonFile(secretsFile, true);
        configuration.AddJsonFile(testDataFile, true);
    })
    .ConfigureServices((ctx, services) =>
    {
        services.AddInfrastructureLayer(ctx.Configuration);
        services.AddHostedService<SeedDataService>();
    })
    .UseSerilog()
    .Build();

await host.RunAsync();

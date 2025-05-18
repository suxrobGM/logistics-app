using Logistics.DbMigrator.Data;
using Logistics.Infrastructure.EF;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");


var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Configuration.AddJsonFile(Path.Combine(AppContext.BaseDirectory, "fake-dataset.json"), optional: true);

builder.Services.AddInfrastructureLayer(builder.Configuration)
    .AddMasterDatabase()
    .AddTenantDatabase()
    .AddIdentity();

builder.Services.AddHostedService<SeedData>();
var host = builder.Build();
host.Run();

//var host = Host.CreateDefaultBuilder(args)
//    .ConfigureAppConfiguration(configuration =>
//    {
//        var testDataFile = Path.Combine(AppContext.BaseDirectory, "fake-dataset.json");
//        configuration.AddJsonFile(testDataFile, true);
//    })
//    .ConfigureServices((ctx, services) =>
//    {
//        services.AddInfrastructureLayer(ctx.Configuration)
//            .AddMasterDatabase()
//            .AddTenantDatabase()
//            .AddIdentity();
//        services.AddHostedService<SeedData>();
//    })
//    .UseSerilog()
//    .Build();

//await host.RunAsync();

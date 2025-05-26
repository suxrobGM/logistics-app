using Logistics.DbMigrator.Services;
using Logistics.DbMigrator.Workers;
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

builder.Services.AddScoped<PayrollService>();

// These hosted services will run in the order they are registered
builder.Services.AddHostedService<MigrateDatabaseWorker>();
builder.Services.AddHostedService<SeedDatabaseWorker>();
builder.Services.AddHostedService<FakeDataWorker>();

var host = builder.Build();
host.Run();

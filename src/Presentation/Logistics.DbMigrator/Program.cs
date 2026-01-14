using Logistics.DbMigrator.Data;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Services;
using Logistics.DbMigrator.Workers;
using Logistics.Infrastructure;

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
builder.Services.AddSeeders();

// These hosted services will run in the order they are registered
builder.Services.AddHostedService<MigrateDatabaseWorker>();
builder.Services.AddHostedService<SeederOrchestrationWorker>();
builder.Services.AddHostedService<CreateSqlFunctionsWorker>();

builder.Build().Run();

using Logistics.DbMigrator.Data;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Services;
using Logistics.DbMigrator.Workers;
using Logistics.Infrastructure.Persistence;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Configuration.AddJsonFile("fake-dataset.json", true);

builder.Services.AddPersistenceInfrastructure(builder.Configuration)
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

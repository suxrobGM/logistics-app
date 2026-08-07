using Duende.IdentityServer.EntityFramework.DbContexts;
using Duende.IdentityServer.EntityFramework.Options;
using Logistics.Application.Modules.Financial.Payroll.Services;
using Logistics.DbMigrator.Data;
using Logistics.Infrastructure.Persistence.Data;
using Logistics.DbMigrator.Extensions;
using Logistics.DbMigrator.Services;
using Logistics.DbMigrator.Workers;
using Logistics.Application;
using Logistics.Infrastructure.Integrations.FuelCards;
using Logistics.Infrastructure.Payments;
using Logistics.Infrastructure.Persistence;
using Logistics.Infrastructure.Tax;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

Log.Information("Starting up");

var builder = Host.CreateApplicationBuilder(args);

builder.Configuration.AddJsonFile("SeedData/us.json", optional: true);
builder.Configuration.AddJsonFile("SeedData/eu.json", optional: true);
builder.Configuration.AddJsonFile("SeedData/solo.json", optional: true);
builder.Configuration["Tax:Provider"] ??= "manual";

builder.Services.AddPersistenceInfrastructure(builder.Configuration)
    .AddMasterDatabase()
    .AddTenantDatabase()
    .AddIdentity();

// Duende operational store (signing keys + persisted grants) also lives in the master DB;
// its migrations must be applied here, before a redeployed IdentityServer expects the schema.
builder.Services.AddSingleton(new OperationalStoreOptions());
builder.Services.AddDbContext<PersistedGrantDbContext>(options =>
    DuendeOperationalStore.ConfigureDbContext(
        options, builder.Configuration.GetConnectionString("MasterDatabase")));

builder.Services.AddApplicationTaxServices();
builder.Services.AddApplicationFuelCardServices();
builder.Services.AddPaymentsInfrastructure(builder.Configuration);
builder.Services.AddTaxInfrastructure(builder.Configuration);
builder.Services.AddFuelCardIntegrations(builder.Configuration);
builder.Services.AddScoped<PayrollService>();
builder.Services.AddSeeders();

// These hosted services will run in the order they are registered
builder.Services.AddHostedService<MigrateDatabaseWorker>();
builder.Services.AddHostedService<SeederOrchestrationWorker>();
builder.Services.AddHostedService<CreateSqlFunctionsWorker>();

// `--exit` stops the host once the workers above have completed, for one-shot runs
if (args.Contains("--exit"))
{
    builder.Services.AddHostedService<StopApplicationWorker>();
}

builder.Build().Run();

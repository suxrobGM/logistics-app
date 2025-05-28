using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

var stripeSecret = builder.Configuration["Stripe:SecretKey"];

var postgres = builder.AddPostgres("postgres", port: 5433)
    .WithPgAdmin(c => c.WithImage("dpage/pgadmin4:latest"))
    .WithVolume("postgres-data", "/var/lib/postgresql/data");

var masterDb = postgres.AddDatabase("master", "master_logistics");
var tenantDb = postgres.AddDatabase("default-tenant", "default_logistics");

// Runs the migrations for the master and tenant databases
var migrator = builder.AddProject<Projects.Logistics_DbMigrator>("migrator")
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WaitFor(postgres);

var logisticsApi = builder.AddProject<Projects.Logistics_API>("api")
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WaitFor(migrator);

var identityServer = builder.AddProject<Projects.Logistics_IdentityServer>("identity-server")
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WaitFor(migrator);

builder.AddProject<Projects.Logistics_AdminApp>("admin-app")
    .WaitFor(logisticsApi)
    .WaitFor(identityServer);

builder.AddBunApp("office-app", "../../Client/Logistics.OfficeApp", entryPoint: "start", watch: true)
    .WithBunPackageInstallation()
    .WithHttpEndpoint()
    .WithUrl("http://localhost:7003")
    .WaitFor(logisticsApi)
    .WaitFor(identityServer);

// Listen for Stripe webhooks and forward them to the logistics API
builder.AddContainer("stripe-cli", "stripe/stripe-cli:latest")
    .WithEnvironment("STRIPE_API_KEY", stripeSecret)
    .WithEnvironment("STRIPE_DEVICE_NAME", "aspire-dev")
    .WithEntrypoint("stripe")
    .WithArgs(
        "listen",
        "--forward-to", "https://localhost:7000/webhooks/stripe")
    .WaitFor(logisticsApi);

builder.Build().Run();

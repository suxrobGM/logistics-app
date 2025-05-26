var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithVolume("postgres-data", "/var/lib/postgresql/data");

var masterDb = postgres.AddDatabase("master", "master_logistics");
var tenantDb = postgres.AddDatabase("default-tenant", "default_logistics");

// Runs the migrations for the master and tenant databases
builder.AddProject<Projects.Logistics_DbMigrator>("migrator")
       .WithReference(masterDb, "MasterDatabase")
       .WithReference(tenantDb, "DefaultTenantDatabase")
       .WaitFor(postgres);

var logisticsApi = builder.AddProject<Projects.Logistics_API>("api")
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WaitFor(postgres);

var identityServer = builder.AddProject<Projects.Logistics_IdentityServer>("identity-server")
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WaitFor(postgres);

builder.AddProject<Projects.Logistics_AdminApp>("admin-app")
    .WaitFor(logisticsApi)
    .WaitFor(identityServer);

builder.AddBunApp("office-app", "../../Client/Logistics.OfficeApp", entryPoint: "start")
    .WithBunPackageInstallation()
    .WithHttpEndpoint()
    .WithUrl("http://localhost:7003")
    .WaitFor(logisticsApi)
    .WaitFor(identityServer);

builder.Build().Run();

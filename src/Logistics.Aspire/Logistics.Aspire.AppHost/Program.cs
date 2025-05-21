
var builder = DistributedApplication.CreateBuilder(args);

var postgres = builder.AddPostgres("postgres")
    .WithPgAdmin()
    .WithVolume("postgres-data", "/var/lib/postgresql/data");

var masterDb = postgres.AddDatabase("master");
var tenantDb = postgres.AddDatabase("defaultTenant");

//uncomment the following line to enable the database migrator
//builder.AddProject<Projects.Logistics_MigrationWorker>("dbMigrator")
//        .WithReference(masterDb, "MasterDatabase")
//        .WithReference(tenantDb, "DefaultTenantDatabase")
//        .WaitFor(postgres);


//uncomment the following line to enable the database seeder
//builder.AddProject<Projects.Logistics_DbMigrator>("dbSeeder")
//       .WithReference(masterDb, "MasterDatabase")
//       .WithReference(tenantDb, "DefaultTenantDatabase")
//       .WaitFor(postgres);


var logisticsApi = builder.AddProject<Projects.Logistics_API>("logisticsApi")
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WaitFor(postgres);

var identityServer = builder.AddProject<Projects.Logistics_IdentityServer>("identityServer")
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WaitFor(postgres);

builder.AddProject<Projects.Logistics_AdminApp>("adminApp")
    .WaitFor(logisticsApi)
    .WaitFor(identityServer);

builder.AddNpmApp("officeApp", "../../Client/Logistics.OfficeApp", "aspire")
    .WaitFor(logisticsApi)
    .WaitFor(identityServer)
    .WithEndpoint(port: 7003, scheme: "http", env: "PORT");



builder.Build().Run();



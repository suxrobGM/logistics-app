using Logistics.Infrastructure.EF;
using Logistics.MigrationWorker;


var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddInfrastructureLayer(builder.Configuration)
    .AddMasterDatabase()
    .AddTenantDatabase()
    .AddIdentity();
builder.Services.AddHostedService<MigrationWorker>();
var host = builder.Build();
host.Run();
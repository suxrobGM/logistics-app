using Logistics.Aspire.AppHost;
using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true, true);

builder.AddDockerComposeEnvironment("compose")
    .WithDashboard(dashboard => dashboard.WithHostPort(7100));

var postgres = builder.AddPostgres("postgres", port: 5433)
    .WithImage("postgres:latest")
    .WithPgAdmin(container =>
        container.WithImage("dpage/pgadmin4:latest").WithHostPort(5434))
    .WithVolume("logistics-pg-data", "/var/lib/postgresql"); // PostgreSQL 18+ uses subdirectories

var masterDb = postgres.AddDatabase("master", "master_logistics");
var tenantDb = postgres.AddDatabase("default-tenant", "default_logistics");

// Runs the migrations for the "master" and tenant databases
var migrator = builder.AddProject<Logistics_DbMigrator>("migrator")
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WithEnvironment("SuperAdmin__Email", builder.GetConfigValue("SuperAdmin:Email"))
    .WithEnvironment("SuperAdmin__Password", builder.GetConfigValue("SuperAdmin:Password"))
    .WithEnvironment("TenantsDatabaseConfig__DatabasePassword", builder.GetConfigValue("TenantsDatabaseConfig:DatabasePassword"))
    .WaitFor(postgres);

var identityServer = builder.AddProject<Logistics_IdentityServer>("identity-server")
    .WithExternalHttpEndpoints()
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WithEnvironment("GoogleRecaptcha__SecretKey", builder.GetConfigValue("GoogleRecaptcha:SecretKey"))
    .WithEnvironment("GoogleRecaptcha__SiteKey", builder.GetConfigValue("GoogleRecaptcha:SiteKey"))
    .WithEnvironment("Smtp__SenderEmail", builder.GetConfigValue("Smtp:SenderEmail"))
    .WithEnvironment("Smtp__SenderName", builder.GetConfigValue("Smtp:SenderName"))
    .WithEnvironment("Smtp__UserName", builder.GetConfigValue("Smtp:UserName"))
    .WithEnvironment("Smtp__Password", builder.GetConfigValue("Smtp:Password"))
    .WithEnvironment("Smtp__Host", builder.GetConfigValue("Smtp:Host"))
    .WithEnvironment("Smtp__Port", builder.GetConfigValue("Smtp:Port"))
    .WaitFor(migrator);

var logisticsApi = builder.AddProject<Logistics_API>("api")
    .WithExternalHttpEndpoints()
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WithEnvironment("IdentityServer__Authority", "http://identity-server:7001")
    .WithEnvironment("IdentityServer__RequireHttpsMetadata", "false")
    .WithEnvironment("Smtp__SenderEmail", builder.GetConfigValue("Smtp:SenderEmail"))
    .WithEnvironment("Smtp__SenderName", builder.GetConfigValue("Smtp:SenderName"))
    .WithEnvironment("Smtp__UserName", builder.GetConfigValue("Smtp:UserName"))
    .WithEnvironment("Smtp__Password", builder.GetConfigValue("Smtp:Password"))
    .WithEnvironment("Smtp__Host", builder.GetConfigValue("Smtp:Host"))
    .WithEnvironment("Smtp__Port", builder.GetConfigValue("Smtp:Port"))
    .WithEnvironment("Stripe__SecretKey", builder.GetConfigValue("Stripe:SecretKey"))
    .WithEnvironment("Stripe__PublishableKey", builder.GetConfigValue("Stripe:PublishableKey"))
    .WithEnvironment("Stripe__WebhookSecret", builder.GetConfigValue("Stripe:WebhookSecret"))
    .WithEnvironment("Mapbox__AccessToken", builder.GetConfigValue("Mapbox:AccessToken"))
    .WithEnvironment("TenantsDatabaseConfig__DatabasePassword", builder.GetConfigValue("TenantsDatabaseConfig:DatabasePassword"))
    .WaitFor(migrator)
    .WaitFor(identityServer);

builder.AddProject<Logistics_AdminApp>("admin-app")
    .WithExternalHttpEndpoints()
    .WaitFor(logisticsApi)
    .WaitFor(identityServer);

// Office App: Use BunApp for local dev, Container for publishing
if (builder.ExecutionContext.IsPublishMode)
{
    // For production: use pre-built container image
    builder.AddContainer("office-app", "ghcr.io/suxrobgm/logistics-app/office")
        .WithImageTag("latest")
        .WithHttpEndpoint(7003, 7003, "office-http")
        .WithExternalHttpEndpoints()
        .WaitFor(logisticsApi)
        .WaitFor(identityServer);
}
else
{
    // For local development: use Bun dev server
    builder.AddBunApp("office-app", "../../Client/Logistics.OfficeApp", "start", true)
        .WithHttpEndpoint(7003, 7003, "office-app-http", isProxied: false)
        .WithBunPackageInstallation()
        .WaitFor(logisticsApi)
        .WaitFor(identityServer);
}

// Listen for Stripe webhooks and forward them to the logistics API
builder.AddContainer("stripe-cli", "stripe/stripe-cli:latest")
    .WithEnvironment("STRIPE_API_KEY", builder.GetConfigValue("Stripe:SecretKey"))
    .WithEnvironment("STRIPE_DEVICE_NAME", "aspire-dev")
    .WithEntrypoint("stripe")
    .WithArgs(
        "listen",
        "--forward-to", "http://api:7000/webhooks/stripe")
    .WaitFor(logisticsApi);

// Portainer Agent: Exposes Docker API to Portainer CE
var portainerAgent = builder.AddContainer("portainer-agent", "portainer/agent:latest")
    .WithVolume("portainer-agent-data", "/var/lib/docker/volumes")
    .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock", true)
    .WithEndpoint(9001, 9001, name: "portainer-agent")
    .WithExternalHttpEndpoints();

// Portainer CE: Web UI for container management
builder.AddContainer("portainer", "portainer/portainer-ce:latest")
    .WithVolume("portainer-data", "/data")
    .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock", true)
    .WithHttpEndpoint(9000, 9000, "portainer-http")
    .WithExternalHttpEndpoints()
    .WaitFor(portainerAgent);

builder.Build().Run();

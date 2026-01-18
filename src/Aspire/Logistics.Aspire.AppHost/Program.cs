using Logistics.Aspire.AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);
var isPublishMode = builder.ExecutionContext.IsPublishMode;

builder.AddDockerComposeEnvironment("compose")
    .WithDashboard(dashboard => dashboard.WithHostPort(7100));

var postgres = builder.AddPostgres("postgres", port: 5433)
    .WithImage("postgres:latest")
    .WithPgAdmin(container =>
        container.WithImage("dpage/pgadmin4:latest").WithHostPort(5434))
    .WithVolume("logistics-pg-data", "/var/lib/postgresql")
    .WithEndpoint("tcp", endpoint => endpoint.IsExternal = true);

var masterDb = postgres.AddDatabase("master", "master_logistics");
var tenantDb = postgres.AddDatabase("default-tenant", "default_logistics");

// Runs the migrations for the "master" and tenant databases
var migrator = builder.AddProject<Logistics_DbMigrator>("migrator")
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WithEnvironment("SuperAdmin__Email", builder.GetConfigValue("SuperAdmin:Email"))
    .WithEnvironment("SuperAdmin__Password", builder.GetConfigValue("SuperAdmin:Password"))
    .WithEnvironment("TenantsDatabaseConfig__DatabasePassword",
        builder.GetConfigValue("TenantsDatabaseConfig:DatabasePassword"))
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
    .WithEnvironment("IdentityServer__Authority", isPublishMode ? "http://identity-server:7001" : "http://localhost:7001")
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
    .WithEnvironment("TenantsDatabaseConfig__DatabasePassword",
        builder.GetConfigValue("TenantsDatabaseConfig:DatabasePassword"))
    .WaitFor(migrator)
    .WaitFor(identityServer);

// Use BunApp for local dev, Container for publishing
if (isPublishMode)
{
    builder.AddContainer("admin-portal", "ghcr.io/suxrobgm/logistics-app/admin-portal")
        .WithImageTag("latest")
        .WithHttpEndpoint(7002, 80, "admin-http")
        .WithExternalHttpEndpoints()
        .WaitFor(logisticsApi)
        .WaitFor(identityServer);

    builder.AddContainer("tms-portal", "ghcr.io/suxrobgm/logistics-app/tms-portal")
        .WithImageTag("latest")
        .WithHttpEndpoint(7003, 80, "tms-http")
        .WithExternalHttpEndpoints()
        .WaitFor(logisticsApi)
        .WaitFor(identityServer);

    builder.AddContainer("customer-portal", "ghcr.io/suxrobgm/logistics-app/customer-portal")
        .WithImageTag("latest")
        .WithHttpEndpoint(7004, 80, "customer-http")
        .WithExternalHttpEndpoints()
        .WaitFor(logisticsApi)
        .WaitFor(identityServer);
}
else
{
    builder.AddBunApp("admin-portal", "../../Client/Logistics.Angular", "start:admin", true)
        .WithHttpEndpoint(7002, 7002, "admin-http", isProxied: false)
        .WithBunPackageInstallation()
        .WaitFor(logisticsApi)
        .WaitFor(identityServer);

    builder.AddBunApp("tms-portal", "../../Client/Logistics.Angular", "start:tms", true)
        .WithHttpEndpoint(7003, 7003, "tms-http", isProxied: false)
        .WithBunPackageInstallation()
        .WaitFor(logisticsApi)
        .WaitFor(identityServer);

    builder.AddBunApp("customer-portal", "../../Client/Logistics.Angular", "start:customer", true)
        .WithHttpEndpoint(7004, 7004, "customer-http", isProxied: false)
        .WithBunPackageInstallation()
        .WaitFor(logisticsApi)
        .WaitFor(identityServer);
}

// Use Stripe CLI only in development mode, on the prod the webhooks are handled by the Stripe Dashboard
if (!isPublishMode)
{
    // Listen for Stripe webhooks and forward them to the logistics API
    builder.AddContainer("stripe-cli", "stripe/stripe-cli:latest")
        .WithEnvironment("STRIPE_API_KEY", builder.GetConfigValue("Stripe:SecretKey"))
        .WithEnvironment("STRIPE_DEVICE_NAME", "aspire-dev")
        .WithEntrypoint("stripe")
        .WithArgs(
            "listen",
            "--forward-to", "http://api:7000/webhooks/stripe")
        .WaitFor(logisticsApi);
}

// Use portainer only in publish mode, on the dev the containers are managed by Aspire Dashboard
if (isPublishMode)
{
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
}

builder.Build().Run();

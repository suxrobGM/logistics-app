using Logistics.Aspire.AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);
var isProdEnv = builder.ExecutionContext.IsPublishMode;
var isDevEnv = !isProdEnv;

builder.AddDockerComposeEnvironment("compose")
    .WithDashboard(dashboard => dashboard.WithHostPort(7100));

IResourceBuilder<IResourceWithConnectionString> masterDb;
IResourceBuilder<IResourceWithConnectionString> defaultTenantDb;

// Development: use containerized PostgreSQL
// Production: use external (installed) PostgreSQL via connection strings from appsettings
if (isProdEnv)
{
    masterDb = builder.AddConnectionString("MasterDatabase");
    defaultTenantDb = builder.AddConnectionString("DefaultTenantDatabase");
}
else
{
    var postgres = builder.AddPostgres("postgres", port: 5433)
        .WithImage("postgres:latest")
        .WithPgAdmin(container =>
            container.WithImage("dpage/pgadmin4:latest").WithHostPort(5434))
        .WithVolume("logistics-pg-data", "/var/lib/postgresql")
        .WithEndpoint("tcp", endpoint => endpoint.IsExternal = true);

    masterDb = postgres.AddDatabase("master", "master_logisticsx");
    defaultTenantDb = postgres.AddDatabase("default-tenant", "default_logisticsx");
}

var identityServer = builder.AddProject<Logistics_IdentityServer>("identity-server")
    .WithExternalHttpEndpoints()
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(defaultTenantDb, "DefaultTenantDatabase")
    .WithEnvironment("GoogleRecaptcha__SecretKey", builder.GetConfigValue("GoogleRecaptcha:SecretKey"))
    .WithEnvironment("GoogleRecaptcha__SiteKey", builder.GetConfigValue("GoogleRecaptcha:SiteKey"))
    .WithEnvironment("Impersonation__TmsPortalUrl", builder.GetConfigValue("Impersonation:TmsPortalUrl"))
    .WithEnvironment("Authentication__Google__ClientId", builder.GetConfigValue("Authentication:Google:ClientId"))
    .WithEnvironment("Authentication__Google__ClientSecret", builder.GetConfigValue("Authentication:Google:ClientSecret"))
    .WithEnvironment("Resend__ApiKey", builder.GetConfigValue("Resend:ApiKey"))
    .WithEnvironment("Resend__SenderEmail", builder.GetConfigValue("Resend:SenderEmail"))
    .WithEnvironment("Resend__SenderName", builder.GetConfigValue("Resend:SenderName"));

var logisticsApi = builder.AddProject<Logistics_API>("api")
    .WithExternalHttpEndpoints()
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(defaultTenantDb, "DefaultTenantDatabase")
    .WithEnvironment("IdentityServer__Authority",
        isProdEnv ? "http://identity-server:7001" : "http://localhost:7001")
    .WithEnvironment("IdentityServer__ExternalAuthority",
        isProdEnv ? builder.GetConfigValue("IdentityServer:ExternalAuthority") : "http://localhost:7001")
    .WithEnvironment("IdentityServer__RequireHttpsMetadata", "false")
    .WithEnvironment("Impersonation__MasterPassword", builder.GetConfigValue("Impersonation:MasterPassword"))
    .WithEnvironment("Resend__ApiKey", builder.GetConfigValue("Resend:ApiKey"))
    .WithEnvironment("Resend__SenderEmail", builder.GetConfigValue("Resend:SenderEmail"))
    .WithEnvironment("Resend__SenderName", builder.GetConfigValue("Resend:SenderName"))
    .WithEnvironment("Stripe__SecretKey", builder.GetConfigValue("Stripe:SecretKey"))
    .WithEnvironment("Stripe__WebhookSecret", builder.GetConfigValue("Stripe:WebhookSecret"))
    .WithEnvironment("Mapbox__AccessToken", builder.GetConfigValue("Mapbox:AccessToken"))
    .WithEnvironment("Llm__Providers__Anthropic__ApiKey", builder.GetConfigValue("Llm:Providers:Anthropic:ApiKey"))
    .WithEnvironment("Llm__Providers__OpenAi__ApiKey", builder.GetConfigValue("Llm:Providers:OpenAi:ApiKey"))
    .WithEnvironment("Llm__Providers__DeepSeek__ApiKey", builder.GetConfigValue("Llm:Providers:DeepSeek:ApiKey"))
    .WithEnvironment("CustomerPortal__BaseUrl", builder.GetConfigValue("CustomerPortal:BaseUrl"))
    .WithEnvironment("FileBlobStorage__BaseUrl", builder.GetConfigValue("FileBlobStorage:BaseUrl"))
    .WithEnvironment("TenantsDatabaseConfig__DatabaseNameTemplate",
        builder.GetConfigValue("TenantsDatabaseConfig:DatabaseNameTemplate"))
    .WithEnvironment("TenantsDatabaseConfig__DatabaseHost",
        builder.GetConfigValue("TenantsDatabaseConfig:DatabaseHost"))
    .WithEnvironment("TenantsDatabaseConfig__DatabasePort",
        builder.GetConfigValue("TenantsDatabaseConfig:DatabasePort"))
    .WithEnvironment("TenantsDatabaseConfig__DatabaseUserId",
        builder.GetConfigValue("TenantsDatabaseConfig:DatabaseUserId"))
    .WithEnvironment("TenantsDatabaseConfig__DatabasePassword",
        builder.GetConfigValue("TenantsDatabaseConfig:DatabasePassword"))
    .WaitFor(identityServer);

// Development only: run the migrator before starting services
if (isDevEnv)
{
    var migrator = builder.AddProject<Logistics_DbMigrator>("migrator")
        .WithReference(masterDb, "MasterDatabase")
        .WithReference(defaultTenantDb, "DefaultTenantDatabase")
        .WithEnvironment("SuperAdmin__Email", builder.GetConfigValue("SuperAdmin:Email"))
        .WithEnvironment("SuperAdmin__Password", builder.GetConfigValue("SuperAdmin:Password"))
        .WithEnvironment("TenantsDatabaseConfig__DatabaseNameTemplate",
            builder.GetConfigValue("TenantsDatabaseConfig:DatabaseNameTemplate"))
        .WithEnvironment("TenantsDatabaseConfig__DatabaseHost",
            builder.GetConfigValue("TenantsDatabaseConfig:DatabaseHost"))
        .WithEnvironment("TenantsDatabaseConfig__DatabasePort",
            builder.GetConfigValue("TenantsDatabaseConfig:DatabasePort"))
        .WithEnvironment("TenantsDatabaseConfig__DatabaseUserId",
            builder.GetConfigValue("TenantsDatabaseConfig:DatabaseUserId"))
        .WithEnvironment("TenantsDatabaseConfig__DatabasePassword",
            builder.GetConfigValue("TenantsDatabaseConfig:DatabasePassword"));

    identityServer.WaitForCompletion(migrator);
    logisticsApi.WaitForCompletion(migrator);
}

// Use BunApp for local dev, Container for publishing
if (isProdEnv)
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
        .WithEnvironment("MAPBOX_TOKEN", builder.GetConfigValue("Mapbox:AccessToken"))
        .WaitFor(logisticsApi)
        .WaitFor(identityServer);

    builder.AddContainer("customer-portal", "ghcr.io/suxrobgm/logistics-app/customer-portal")
        .WithImageTag("latest")
        .WithHttpEndpoint(7004, 80, "customer-http")
        .WithExternalHttpEndpoints()
        .WaitFor(logisticsApi)
        .WaitFor(identityServer);

    builder.AddContainer("website", "ghcr.io/suxrobgm/logistics-app/website")
        .WithImageTag("latest")
        .WithHttpEndpoint(7005, 7005, "website-http")
        .WithExternalHttpEndpoints()
        .WaitFor(logisticsApi);
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

    builder.AddBunApp("website", "../../Client/Logistics.Angular", "start:website", true)
        .WithHttpEndpoint(7005, 7005, "website-http", isProxied: false)
        .WithBunPackageInstallation()
        .WaitFor(logisticsApi);
}

// Use Stripe CLI only in development mode, on the prod the webhooks are handled by the Stripe Dashboard
if (isDevEnv)
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
if (isProdEnv)
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

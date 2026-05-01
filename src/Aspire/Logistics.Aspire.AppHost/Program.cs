using Logistics.Aspire.AppHost;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);
var isProdEnv = builder.ExecutionContext.IsPublishMode;
var isDevEnv = !isProdEnv;

builder.AddDockerComposeEnvironment("compose")
    .WithDashboard(dashboard => dashboard.WithHostPort(7100));

IResourceBuilder<IResourceWithConnectionString> masterDb;
IResourceBuilder<IResourceWithConnectionString> usTenantDb;
IResourceBuilder<IResourceWithConnectionString> euTenantDb;

// Development: use containerized PostgreSQL
// Production: use external (installed) PostgreSQL via connection strings from appsettings
if (isProdEnv)
{
    masterDb = builder.AddConnectionString("MasterDatabase");
    usTenantDb = builder.AddConnectionString("UsTenantDatabase");
    euTenantDb = builder.AddConnectionString("EuTenantDatabase");
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
    usTenantDb = postgres.AddDatabase("us-tenant", "us_logisticsx");
    euTenantDb = postgres.AddDatabase("eu-tenant", "eu_logisticsx");
}

var identityServer = builder.AddProject<Logistics_IdentityServer>("identity-server")
    .WithExternalHttpEndpoints()
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(usTenantDb, "UsTenantDatabase")
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
    .WithReference(usTenantDb, "UsTenantDatabase")
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
    .WithEnvironment("TelegramBot__BotToken", builder.GetConfigValue("TelegramBot:BotToken"))
    .WithEnvironment("TelegramBot__WebhookUrl", builder.GetConfigValue("TelegramBot:WebhookUrl"))
    .WithEnvironment("TelegramBot__SecretToken", builder.GetConfigValue("TelegramBot:SecretToken"))
    .WithEnvironment("TelegramBot__IdentityServerUrl", builder.GetConfigValue("TelegramBot:IdentityServerUrl"))
    .WithEnvironment("CustomerPortal__BaseUrl", builder.GetConfigValue("CustomerPortal:BaseUrl"))
    .WithEnvironment("BlobStorage__Type", builder.GetConfigValue("BlobStorage:Type"))
    .WithEnvironment("FileBlobStorage__BaseUrl", builder.GetConfigValue("FileBlobStorage:BaseUrl"))
    .WithEnvironment("ConnectionStrings__AzureBlobStorage",
        builder.GetConfigValue("ConnectionStrings:AzureBlobStorage"))
    .WithEnvironment("AzureBlobStorage__DefaultContainer",
        builder.GetConfigValue("AzureBlobStorage:DefaultContainer"))
    .WithEnvironment("R2BlobStorage__AccountId", builder.GetConfigValue("R2BlobStorage:AccountId"))
    .WithEnvironment("R2BlobStorage__AccessKeyId", builder.GetConfigValue("R2BlobStorage:AccessKeyId"))
    .WithEnvironment("R2BlobStorage__SecretAccessKey", builder.GetConfigValue("R2BlobStorage:SecretAccessKey"))
    .WithEnvironment("R2BlobStorage__BucketName", builder.GetConfigValue("R2BlobStorage:BucketName"))
    .WithEnvironment("R2BlobStorage__PublicBaseUrl", builder.GetConfigValue("R2BlobStorage:PublicBaseUrl"))
    .WithEnvironment("TenantDatabaseDefaults__NameTemplate",
        builder.GetConfigValue("TenantDatabaseDefaults:NameTemplate"))
    .WithEnvironment("TenantDatabaseDefaults__Host",
        builder.GetConfigValue("TenantDatabaseDefaults:Host"))
    .WithEnvironment("TenantDatabaseDefaults__Port",
        builder.GetConfigValue("TenantDatabaseDefaults:Port"))
    .WithEnvironment("TenantDatabaseDefaults__UserId",
        builder.GetConfigValue("TenantDatabaseDefaults:UserId"))
    .WithEnvironment("TenantDatabaseDefaults__Password",
        builder.GetConfigValue("TenantDatabaseDefaults:Password"))
    .WaitFor(identityServer);

// Development only: run the migrator before starting services
if (isDevEnv)
{
    var migrator = builder.AddProject<Logistics_DbMigrator>("migrator")
        .WithReference(masterDb, "MasterDatabase")
        .WithReference(usTenantDb, "UsTenantDatabase")
        .WithReference(euTenantDb, "EuTenantDatabase")
        .WithEnvironment("SuperAdmin__Email", builder.GetConfigValue("SuperAdmin:Email"))
        .WithEnvironment("SuperAdmin__Password", builder.GetConfigValue("SuperAdmin:Password"))
        .WithEnvironment("Tenants__0__Name", "us")
        .WithEnvironment("Tenants__0__CompanyName", "Heartland Logistics LLC")
        .WithEnvironment("Tenants__0__BillingEmail", "billing@heartlandlogistics.com")
        .WithEnvironment("Tenants__0__Region", "Us")
        .WithEnvironment("Tenants__1__Name", "eu")
        .WithEnvironment("Tenants__1__CompanyName", "EuroFreight GmbH")
        .WithEnvironment("Tenants__1__BillingEmail", "billing@eurofreight.de")
        .WithEnvironment("Tenants__1__Region", "Eu")
        .WithEnvironment("TenantDatabaseDefaults__NameTemplate",
            builder.GetConfigValue("TenantDatabaseDefaults:NameTemplate"))
        .WithEnvironment("TenantDatabaseDefaults__Host",
            builder.GetConfigValue("TenantDatabaseDefaults:Host"))
        .WithEnvironment("TenantDatabaseDefaults__Port",
            builder.GetConfigValue("TenantDatabaseDefaults:Port"))
        .WithEnvironment("TenantDatabaseDefaults__UserId",
            builder.GetConfigValue("TenantDatabaseDefaults:UserId"))
        .WithEnvironment("TenantDatabaseDefaults__Password",
            builder.GetConfigValue("TenantDatabaseDefaults:Password"));

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

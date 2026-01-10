using Microsoft.Extensions.Configuration;
using Projects;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", true, true);

var stripeSecret = builder.Configuration["Stripe:SecretKey"];
var domain = builder.Configuration["Domain"] ?? "localhost";
var enableNginx = builder.Configuration.GetValue<bool>("EnableNginx");

builder.AddDockerComposeEnvironment("compose")
    .WithDashboard(dashboard => dashboard.WithHostPort(7100));

var postgres = builder.AddPostgres("postgres", port: 5433)
    .WithImage("postgres:latest")
    .WithPgAdmin(config =>
        config.WithImage("dpage/pgadmin4:latest").WithHostPort(5434))
    .WithVolume("logistics-pg-data", "/var/lib/postgresql"); // PostgreSQL 18+ uses subdirectories

var masterDb = postgres.AddDatabase("master", "master_logistics");
var tenantDb = postgres.AddDatabase("default-tenant", "default_logistics");

// Runs the migrations for the "master" and tenant databases
var migrator = builder.AddProject<Logistics_DbMigrator>("migrator")
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WaitFor(postgres);

var logisticsApi = builder.AddProject<Logistics_API>("api")
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WaitFor(migrator);

var identityServer = builder.AddProject<Logistics_IdentityServer>("identity-server")
    .WithReference(masterDb, "MasterDatabase")
    .WithReference(tenantDb, "DefaultTenantDatabase")
    .WaitFor(migrator);

var adminApp = builder.AddProject<Logistics_AdminApp>("admin-app")
    .WaitFor(logisticsApi)
    .WaitFor(identityServer);

// Office App: Use BunApp for local dev, Container for publishing
IResourceBuilder<IResourceWithEnvironment> officeApp;
if (builder.ExecutionContext.IsPublishMode)
{
    // For production: use pre-built container image
    officeApp = builder.AddContainer("office-app", "ghcr.io/suxrobgm/logistics-app/office")
        .WithImageTag("latest")
        .WithHttpEndpoint(7003, 7003, "office-http")
        .WaitFor(logisticsApi)
        .WaitFor(identityServer);
}
else
{
    // For local development: use Bun dev server
    officeApp = builder.AddBunApp("office-app", "../../Client/Logistics.OfficeApp", "start", true)
        .WithHttpEndpoint(7003, 7003, "office-app-http", isProxied: false)
        .WithBunPackageInstallation()
        .WaitFor(logisticsApi)
        .WaitFor(identityServer);
}

// Add nginx-proxy virtual host labels when nginx is enabled
if (enableNginx)
{
    logisticsApi
        .WithEnvironment("VIRTUAL_HOST", $"api.{domain}")
        .WithEnvironment("LETSENCRYPT_HOST", $"api.{domain}");

    identityServer
        .WithEnvironment("VIRTUAL_HOST", $"id.{domain}")
        .WithEnvironment("LETSENCRYPT_HOST", $"id.{domain}");

    adminApp
        .WithEnvironment("VIRTUAL_HOST", $"admin.{domain}")
        .WithEnvironment("LETSENCRYPT_HOST", $"admin.{domain}");

    officeApp
        .WithEnvironment("VIRTUAL_HOST", $"office.{domain}")
        .WithEnvironment("LETSENCRYPT_HOST", $"office.{domain}");
}

// Listen for Stripe webhooks and forward them to the logistics API
builder.AddContainer("stripe-cli", "stripe/stripe-cli:latest")
    .WithEnvironment("STRIPE_API_KEY", stripeSecret)
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
    .WithEndpoint(9001, 9001, name: "portainer-agent");

// Portainer CE: Web UI for container management
builder.AddContainer("portainer", "portainer/portainer-ce:latest")
    .WithVolume("portainer-data", "/data")
    .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock", true)
    .WithHttpEndpoint(9000, 9000, "portainer-http")
    .WaitFor(portainerAgent);

// Production: nginx-proxy with automatic SSL via acme-companion
// Enable by setting EnableNginx=true and Domain=yourdomain.com in appsettings
if (enableNginx)
{
    var letsencryptEmail = builder.Configuration["LetsEncryptEmail"] ?? $"admin@{domain}";

    // nginx-proxy: automatic reverse proxy based on VIRTUAL_HOST env vars
    var nginxProxy = builder.AddContainer("nginx-proxy", "nginxproxy/nginx-proxy:latest")
        .WithContainerName("nginx-proxy")
        .WithVolume("nginx-certs", "/etc/nginx/certs")
        .WithVolume("nginx-vhost", "/etc/nginx/vhost.d")
        .WithVolume("nginx-html", "/usr/share/nginx/html")
        .WithBindMount("/var/run/docker.sock", "/tmp/docker.sock", true)
        .WithHttpEndpoint(80, 80, "nginx-http")
        .WithHttpsEndpoint(443, 443, "nginx-https")
        .WithExternalHttpEndpoints();

    // acme-companion: automatic Let's Encrypt SSL certificates
    builder.AddContainer("acme-companion", "nginxproxy/acme-companion:latest")
        .WithContainerName("acme-companion")
        .WithEnvironment("DEFAULT_EMAIL", letsencryptEmail)
        .WithEnvironment("NGINX_PROXY_CONTAINER", "nginx-proxy")
        .WithVolume("nginx-certs", "/etc/nginx/certs")
        .WithVolume("nginx-vhost", "/etc/nginx/vhost.d")
        .WithVolume("nginx-html", "/usr/share/nginx/html")
        .WithVolume("acme-state", "/etc/acme.sh")
        .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock", true)
        .WaitFor(nginxProxy);
}

builder.Build().Run();

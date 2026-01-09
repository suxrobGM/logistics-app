using Microsoft.Extensions.Configuration;

var builder = DistributedApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

var stripeSecret = builder.Configuration["Stripe:SecretKey"];
var domain = builder.Configuration["Domain"] ?? "localhost";
var enableNginx = builder.Configuration.GetValue<bool>("EnableNginx");

//builder.AddDockerComposeEnvironment("compose");

var postgres = builder.AddPostgres("postgres", port: 5433)
    .WithImage("postgres:latest")
    .WithPgAdmin(config =>
        config.WithImage("dpage/pgadmin4:latest").WithHostPort(5434))
    .WithVolume("logistics-pg-data", "/var/lib/postgresql"); // PostgreSQL 18+ uses subdirectories

var masterDb = postgres.AddDatabase("master", "master_logistics");
var tenantDb = postgres.AddDatabase("default-tenant", "default_logistics");

// Runs the migrations for the "master" and tenant databases
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

var adminApp = builder.AddProject<Projects.Logistics_AdminApp>("admin-app")
    .WaitFor(logisticsApi)
    .WaitFor(identityServer);

var officeApp = builder.AddBunApp("office-app", "../../Client/Logistics.OfficeApp", entryPoint: "start", watch: true)
    .WithBunPackageInstallation()
    .WithHttpEndpoint()
    .WithUrl("http://localhost:7003")
    .WaitFor(logisticsApi)
    .WaitFor(identityServer);

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
        "--forward-to", "http://localhost:7000/webhooks/stripe")
    .WaitFor(logisticsApi);

// Production: nginx-proxy with automatic SSL via acme-companion
// Enable by setting EnableNginx=true and Domain=yourdomain.com in appsettings
if (enableNginx)
{
    var letsencryptEmail = builder.Configuration["LetsEncryptEmail"] ?? $"admin@{domain}";

    // nginx-proxy: automatic reverse proxy based on VIRTUAL_HOST env vars
    var nginxProxy = builder.AddContainer("nginx-proxy", "nginxproxy/nginx-proxy:latest")
        .WithVolume("nginx-certs", "/etc/nginx/certs")
        .WithVolume("nginx-vhost", "/etc/nginx/vhost.d")
        .WithVolume("nginx-html", "/usr/share/nginx/html")
        .WithBindMount("/var/run/docker.sock", "/tmp/docker.sock", isReadOnly: true)
        .WithHttpEndpoint(port: 80, targetPort: 80, name: "http")
        .WithHttpsEndpoint(port: 443, targetPort: 443, name: "https");

    // acme-companion: automatic Let's Encrypt SSL certificates
    builder.AddContainer("acme-companion", "nginxproxy/acme-companion:latest")
        .WithEnvironment("DEFAULT_EMAIL", letsencryptEmail)
        .WithVolume("nginx-certs", "/etc/nginx/certs")
        .WithVolume("nginx-vhost", "/etc/nginx/vhost.d")
        .WithVolume("nginx-html", "/usr/share/nginx/html")
        .WithVolume("acme-state", "/etc/acme.sh")
        .WithBindMount("/var/run/docker.sock", "/var/run/docker.sock", isReadOnly: true)
        .WaitFor(nginxProxy);
}

builder.Build().Run();

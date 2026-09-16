using System.Threading.RateLimiting;
using Open.IdentityServer;
using Logistics.Application;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.HostDefaults;
using Logistics.IdentityServer.Services;
using Logistics.IdentityServer.Services.SigningKeys;
using Open.IdentityServer.Stores;
using Logistics.Infrastructure.Communications;
using Logistics.Infrastructure.Persistence;
using Logistics.Infrastructure.Persistence.Data;
using Logistics.Infrastructure.Storage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Serilog;

namespace Logistics.IdentityServer;

internal static class Setup
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        // Configuration options
        services.Configure<ImpersonationOptions>(configuration.GetSection(ImpersonationOptions.SectionName));

        // Infrastructure layers
        services.AddCommunicationsInfrastructure(configuration);
        services.AddPersistenceInfrastructure(configuration)
            .AddMasterDatabase()
            .AddTenantDatabase()
            .AddIdentity(identityBuilder =>
            {
                identityBuilder
                    .AddSignInManager()
                    .AddClaimsPrincipalFactory<UserCustomClaimsFactory>()
                    .AddDefaultTokenProviders();
            });

        // Backs the signed download URL on the Manage Privacy page's data exports.
        services.AddStorageInfrastructure(configuration);

        // Application services used directly by Razor pages. Still not the full Application/MediatR
        // stack - just UserService (Manage Profile) and the Privacy slice (Manage Privacy).
        services.AddScoped<IUserService, UserService>();
        services.AddApplicationPrivacyServices();

        services.AddRazorPages();
        AddAuthSchemes(services);

        // The application name feeds purpose derivation for every protected payload, including
        // the auth cookies and the signing keys. Changing it makes all of them unreadable.
        services.AddLogisticsDataProtection<MasterDbContext>("LogisticsX.IdentityServer");

        // Real health probe: master DB connectivity (composes with AddHealthChecks() in LogisticsHost).
        services.AddHealthChecks().AddDbContextCheck<MasterDbContext>("master-db");

        // Replaces the automatic key management the Duende package provided. Registered before
        // AddIdentityServer because the first ISigningCredentialStore registered is the one that signs.
        services.Configure<SigningKeyOptions>(configuration.GetSection(SigningKeyOptions.SectionName));
        services.AddScoped<SigningKeyProtector>();
        services.AddSingleton<SigningKeyCache>();
        services.AddScoped<ISigningCredentialStore, RotatingSigningKeyStore>();
        services.AddScoped<IValidationKeysStore, RotatingSigningKeyStore>();
        services.AddSingleton<SigningKeyMaintenance>();
        services.AddHostedService(sp => sp.GetRequiredService<SigningKeyMaintenance>());

        services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources())
            .AddInMemoryApiScopes(Config.ApiScopes())
            .AddInMemoryApiResources(Config.ApiResources())
            .AddInMemoryClients(Config.Clients(configuration))
            .AddAspNetIdentity<User>()
            // Refresh tokens in the master DB; without this they live in the container and
            // every redeploy invalidates all sessions
            .AddOperationalStore(options =>
            {
                OperationalStoreSetup.ConfigureStoreOptions(options);
                options.ConfigureDbContext = b => OperationalStoreSetup.ConfigureDbContext(
                    b, configuration.GetConnectionString("MasterDatabase"));
                options.EnableTokenCleanup = true;
            });

        services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;
                options.ClientId = configuration["Authentication:Google:ClientId"]!;
                options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
            });

        services.AddLogisticsCors("https://*.logisticsx.app");

        // Rate limiting configuration
        services.AddRateLimiter(options =>
        {
            // Rate limit for login attempts per IP
            options.AddPolicy("login", context =>
                RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: _ => new SlidingWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(15),
                        SegmentsPerWindow = 3,
                        QueueLimit = 0
                    }));

            // Rate limit for impersonation token validation
            options.AddIpFixedWindowPolicy("impersonation", 5, TimeSpan.FromMinutes(15));

            // The token endpoint is middleware-generated, so rate-limit it globally by path.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                context.Request.Path.StartsWithSegments("/connect/token")
                    ? RateLimitPartition.GetFixedWindowLimiter(
                        context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 30,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        })
                    : RateLimitPartition.GetNoLimiter("unlimited"));

            options.OnRejected = async (context, ct) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                if (context.HttpContext.Request.Path.StartsWithSegments("/connect/token"))
                {
                    await context.HttpContext.Response.WriteAsync("Too many requests. Please retry later.", ct);
                }
                else
                {
                    context.HttpContext.Response.Redirect("/Account/Login?error=TooManyAttempts");
                }
            };
        });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseLogisticsProductVersionHeader();
        app.UseSerilogRequestLogging();

        if (app.Environment.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();
        app.UseLogisticsCors();

        app.UseRateLimiter();
        app.UseIdentityServer();
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();

        EnsureSigningKey(app);
        return app;
    }

    /// <summary>
    ///     Runs before <c>app.Run()</c> rather than as a hosted service, because Kestrel is
    ///     registered first and would otherwise start serving requests before a key exists.
    /// </summary>
    private static void EnsureSigningKey(WebApplication app)
    {
        var maintenance = app.Services.GetRequiredService<SigningKeyMaintenance>();
        var logger = app.Services.GetRequiredService<ILogger<SigningKeyMaintenance>>();

        for (var attempt = 1; ; attempt++)
        {
            try
            {
                maintenance.EnsureKeysAsync(CancellationToken.None).GetAwaiter().GetResult();
                return;
            }
            catch (Exception ex) when (attempt < 5)
            {
                logger.LogWarning(ex, "Signing key check failed (attempt {Attempt}), retrying", attempt);
                Thread.Sleep(TimeSpan.FromSeconds(attempt * 2));
            }
        }
    }

    private static void AddAuthSchemes(IServiceCollection services)
    {
        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
                options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme, o =>
            {
                o.LoginPath = new PathString("/Account/Login");
                o.Cookie.SameSite = SameSiteMode.None;
                o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                o.Cookie.HttpOnly = true;
                o.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = SecurityStampValidator.ValidatePrincipalAsync
                };
            })
            .AddCookie(IdentityConstants.ExternalScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.ExternalScheme;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5.0);
            })
            .AddCookie(IdentityConstants.TwoFactorRememberMeScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.TwoFactorRememberMeScheme;
                o.Events = new CookieAuthenticationEvents
                {
                    OnValidatePrincipal = SecurityStampValidator.ValidateAsync<ITwoFactorSecurityStampValidator>
                };
            })
            .AddCookie(IdentityConstants.TwoFactorUserIdScheme, o =>
            {
                o.Cookie.Name = IdentityConstants.TwoFactorUserIdScheme;
                o.ExpireTimeSpan = TimeSpan.FromMinutes(5.0);
            });
    }
}

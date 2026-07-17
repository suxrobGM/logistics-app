using System.Threading.RateLimiting;
using Duende.IdentityServer;
using Logistics.Application.Modules.IdentityAccess.Users.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.HostDefaults;
using Logistics.IdentityServer.Services;
using Logistics.Infrastructure.Communications;
using Logistics.Infrastructure.Persistence;
using Logistics.Infrastructure.Persistence.Builder;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Extensions.Logging;

namespace Logistics.IdentityServer;

internal static class Setup
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        var serilogLogger = new SerilogLoggerFactory(Log.Logger)
            .CreateLogger<IPersistenceInfrastructureBuilder>();

        // Configuration options
        services.Configure<ImpersonationOptions>(configuration.GetSection(ImpersonationOptions.SectionName));

        // Infrastructure layers
        services.AddCommunicationsInfrastructure(configuration);
        services.AddPersistenceInfrastructure(configuration)
            .UseLogger(serilogLogger)
            .AddMasterDatabase()
            .AddTenantDatabase()
            .AddIdentity(identityBuilder =>
            {
                identityBuilder
                    .AddSignInManager()
                    .AddClaimsPrincipalFactory<UserCustomClaimsFactory>()
                    .AddDefaultTokenProviders();
            });

        // Application services used directly by Razor pages (e.g. the Manage Profile page).
        // The IdentityServer doesn't pull in the full Application/MediatR stack, so register
        // only what it needs (UserService depends solely on the master/tenant unit of work).
        services.AddScoped<IUserService, UserService>();

        services.AddRazorPages();
        AddAuthSchemes(services);

        // Explicit app name (previously unset). This invalidates cookies protected before this
        // change — a one-time forced re-login; there are no persisted grants to lose.
        services.AddLogisticsDataProtection<MasterDbContext>("LogisticsX.IdentityServer");

        // Real health probe: master DB connectivity (composes with AddHealthChecks() in LogisticsHost).
        services.AddHealthChecks().AddDbContextCheck<MasterDbContext>("master-db");

        services.AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources())
            .AddInMemoryApiScopes(Config.ApiScopes())
            .AddInMemoryApiResources(Config.ApiResources())
            .AddInMemoryClients(Config.Clients(configuration))
            .AddAspNetIdentity<User>();

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

            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.Redirect("/Account/Login?error=TooManyAttempts");
                await Task.CompletedTask;
            };
        });

        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
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
        return app;
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

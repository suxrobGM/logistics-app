using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Logging;
using Duende.IdentityServer;
using Serilog;
using Logistics.EntityFramework;
using Logistics.EntityFramework.Data;
using Logistics.IdentityServer.Services;

namespace Logistics.IdentityServer;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        IdentityModelEventSource.ShowPII = true;
        AddSecretsJson(builder.Configuration);
        builder.Services.AddRazorPages();
        builder.Services.AddInfrastructureLayer(builder.Configuration, "LocalMainDatabase");
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddIdentity<User, IdentityRole>()
            .AddEntityFrameworkStores<MainDbContext>()
            .AddDefaultTokenProviders();

        builder.Services
            .AddIdentityServer(options =>
            {
                options.Events.RaiseErrorEvents = true;
                options.Events.RaiseInformationEvents = true;
                options.Events.RaiseFailureEvents = true;
                options.Events.RaiseSuccessEvents = true;

                // see https://docs.duendesoftware.com/identityserver/v6/fundamentals/resources/
                options.EmitStaticAudienceClaim = true;
            })
            .AddInMemoryIdentityResources(Config.IdentityResources())
            .AddInMemoryApiScopes(Config.ApiScopes(builder.Configuration))
            .AddInMemoryApiResources(Config.ApiResources(builder.Configuration))
            .AddInMemoryClients(Config.Clients(builder.Configuration))
            .AddAspNetIdentity<User>()
            .AddProfileService<UserProfileService>();

        builder.Services.AddAuthentication()
            .AddGoogle(options =>
            {
                options.SignInScheme = IdentityServerConstants.ExternalCookieAuthenticationScheme;

                // register your IdentityServer with Google at https://console.developers.google.com
                // enable the Google+ API
                // set the redirect URI to https://localhost:5001/signin-google
                options.ClientId = "copy client ID from Google here";
                options.ClientSecret = "copy client secret from Google here";
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

        app.UseStaticFiles();
        app.UseRouting();
        app.UseIdentityServer();
        app.UseAuthorization();
        app.MapRazorPages().RequireAuthorization();
        return app;
    }
    
    private static void AddSecretsJson(IConfigurationBuilder configuration)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "secrets.json");
        configuration.AddJsonFile(path, true);
    }
}
using System.Security.Claims;
using Logistics.AdminApp.Authorization;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;
using Microsoft.AspNetCore.Authorization;
using Logistics.Shared.Claims;

namespace Logistics.AdminApp;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        AddSecretsJson(builder.Configuration);
        builder.Services.AddWebApiClient(builder.Configuration);
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = "Cookies";
            options.DefaultSignInScheme = "Cookies";
            options.DefaultChallengeScheme = "oidc";
        })
        .AddCookie("Cookies")
        .AddOpenIdConnect("oidc", options =>
        {
            builder.Configuration.Bind("OidcClient", options);
            options.ResponseType = "code";

            options.GetClaimsFromUserInfoEndpoint = true;
            options.MapInboundClaims = false;
            options.SaveTokens = true;
            options.ClaimActions.Add(new JsonKeyClaimAction(CustomClaimTypes.Permission, ClaimValueTypes.String, "permission"));
            options.ClaimActions.Add(new JsonKeyClaimAction(ClaimTypes.Role, ClaimValueTypes.String, "role"));
            options.ClaimActions.Add(new JsonKeyClaimAction(ClaimTypes.Name, ClaimValueTypes.String, "name"));
        });

        builder.Services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        builder.Services.AddAuthorization();
        
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
        return app;
    }

    private static void AddSecretsJson(IConfigurationBuilder configuration)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.secrets.json");
        configuration.AddJsonFile(path, true);
    }
}

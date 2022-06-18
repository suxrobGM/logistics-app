using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.OAuth.Claims;

namespace Logistics.OfficeApp;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        AddSecretsJson(builder.Configuration);
        builder.Services.AddWebApiClient(builder.Configuration);
        builder.Services.AddMvvmBlazor();
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
            builder.Configuration.Bind("IdentityServer", options);
            options.ResponseType = "code";

            options.MapInboundClaims = false;
            options.GetClaimsFromUserInfoEndpoint = true;
            options.SaveTokens = true;
            options.ClaimActions.Add(new JsonKeyClaimAction(ClaimTypes.Role, ClaimValueTypes.String, "role"));
            options.ClaimActions.Add(new JsonKeyClaimAction(ClaimTypes.Name, ClaimValueTypes.String, "name"));
        });

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });
        
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
        app.UseMultitenancy();
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
        var path = Path.Combine(AppContext.BaseDirectory, "secrets.json");
        configuration.AddJsonFile(path, true);
    }
}

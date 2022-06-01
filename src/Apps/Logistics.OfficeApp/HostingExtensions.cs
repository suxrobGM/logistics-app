using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace Logistics.OfficeApp;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        AddSecretsJson(builder.Configuration);
        builder.Services.AddWebApiClient(builder.Configuration);
        builder.Services.AddMvvmBlazor();
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            //.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
            .AddMicrosoftIdentityWebApp(c => ConfigureIdentity(builder.Configuration, c));

        builder.Services.AddControllersWithViews()
            .AddMicrosoftIdentityUI();

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });

        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor()
            .AddMicrosoftIdentityConsentHandler();
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

    private static void AddSecretsJson(ConfigurationManager configuration)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "secrets.json");
        configuration.AddJsonFile(path, true);
    }

    private static void ConfigureIdentity(IConfiguration configuration, MicrosoftIdentityOptions options)
    {
        configuration.Bind("AzureAd", options);
        options.Events.OnRedirectToIdentityProvider = c =>
        {
            var tenant = c.HttpContext?.Request?.Cookies["X-Tenant"];
            c.ProtocolMessage.State = tenant;
            var a = c.Properties.Items;
            var b = c.ProtocolMessage.Parameters;
            var d = c.Properties.Parameters;
            return Task.CompletedTask;
        };

        options.Events.OnTicketReceived = c =>
        {
            foreach (var claim in c.Principal.Claims)
            {
                Console.WriteLine(claim.Value);
            }

            return Task.CompletedTask;
        };
    }
}

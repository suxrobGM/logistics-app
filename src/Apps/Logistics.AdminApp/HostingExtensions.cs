using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using MagicMvvm;
using Syncfusion.Blazor;
using Syncfusion.Licensing;
using Logistics.Application;
using Logistics.EntityFramework;
using Logistics.WebApi.Client;

namespace Logistics.AdminApp;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        SyncfusionLicenseProvider.RegisterLicense(builder.Configuration["SyncfusionKey"]);

        builder.Services.AddApplicationLayer(builder.Configuration);
        builder.Services.AddEntityFrameworkLayer(builder.Configuration);
        builder.Services.AddWebApiClient(builder.Configuration);

        builder.Services.AddMvvmBlazor();

        builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
        builder.Services.AddControllersWithViews()
            .AddMicrosoftIdentityUI();

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });

        builder.Services.AddRazorPages();
        builder.Services.AddSyncfusionBlazor(o => o.IgnoreScriptIsolation = true);
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
        app.UseStaticFiles();
        app.UseRouting();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
        return app;
    }
}

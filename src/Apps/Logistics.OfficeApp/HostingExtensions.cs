using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Logistics.OfficeApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

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
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
        .AddOpenIdConnect(options =>
        {
            options.Authority = "https://localhost:5001";
            options.ClientId = "logistics.officeapp";
            options.ClientSecret = "589270E9-2155-4A4F-84E5-CA641695CED2";
            options.ResponseType = "code";
        });
        //.AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));

        //builder.Services.AddControllersWithViews()
        //    .AddMicrosoftIdentityUI();

        builder.Services.AddAuthorization(options =>
        {
            options.FallbackPolicy = options.DefaultPolicy;
        });

        builder.Services.AddScoped<AuthenticationStateService>();
        builder.Services.AddRazorPages();
        builder.Services.AddServerSideBlazor();
            //.AddMicrosoftIdentityConsentHandler();
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
}

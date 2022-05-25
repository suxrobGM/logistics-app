namespace Logistics.AdminApp;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddWebApiClient(builder.Configuration);
        builder.Services.AddMvvmBlazor();

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

        //app.UseAuthentication();
        //app.UseAuthorization();

        app.MapBlazorHub();
        app.MapFallbackToPage("/_Host");
        return app;
    }
}

//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.Identity.Web;
//using Microsoft.Identity.Web.UI;
using Logistics.Application;
using Logistics.EntityFramework;

namespace Logistics.WebApi;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddApplicationLayer();
        builder.Services.AddEntityFrameworkLayer(builder.Configuration);

        //builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
        //    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"));
        //builder.Services.AddControllersWithViews()
        //    .AddMicrosoftIdentityUI();

        //builder.Services.AddAuthorization(options =>
        //{
        //    // By default, all incoming requests will be authorized according to the default policy
        //    options.FallbackPolicy = options.DefaultPolicy;
        //});

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();
        return app;
    }
}

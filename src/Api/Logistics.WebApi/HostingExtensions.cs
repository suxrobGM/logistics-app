//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Logistics.Application;
using Logistics.EntityFramework;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;

namespace Logistics.WebApi;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        AddSecretsJson(builder.Configuration);
        builder.Services.AddMainApplicationLayer();
        builder.Services.AddTenantApplicationLayer(builder.Configuration);
        builder.Services.AddInfrastructureLayer(builder.Configuration, "LocalMainDatabase");

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

        builder.Services.AddControllers(configure =>
        {
            //var policy = new AuthorizationPolicyBuilder()
            //                .RequireAuthenticatedUser()
            //                .Build();

            //configure.Filters.Add(new AuthorizeFilter(policy));
        });
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

        app.UseCors(builder =>
        {
            builder
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
        });

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        return app;
    }

    private static void AddSecretsJson(ConfigurationManager configuration)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "secrets.json");
        configuration.AddJsonFile(path, true);
    }
}

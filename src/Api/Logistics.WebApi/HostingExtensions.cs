using Microsoft.AspNetCore.Mvc.Authorization;
using Logistics.Application;
using Logistics.EntityFramework;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Logging;

namespace Logistics.WebApi;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        IdentityModelEventSource.ShowPII = true;
        AddSecretsJson(builder.Configuration);
        builder.Services.AddMainApplicationLayer();
        builder.Services.AddTenantApplicationLayer();
        builder.Services.AddSharedApplicationLayer(builder.Configuration, "EmailConfig");
        builder.Services.AddInfrastructureLayer(builder.Configuration, "LocalMainDatabase");
        builder.Services.AddHttpContextAccessor();

        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                builder.Configuration.Bind("IdentityServer", options);
                options.TokenValidationParameters.ValidateAudience = true;
            });

        builder.Services.AddControllers(configure =>
        {
            var policy = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();

            configure.Filters.Add(new AuthorizeFilter(policy));
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

    private static void AddSecretsJson(IConfigurationBuilder configuration)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "secrets.json");
        configuration.AddJsonFile(path, true);
    }
}

using System.Text;
using Logistics.API.Authorization;
using Logistics.API.Hubs;
using Logistics.API.Middlewares;
using Logistics.API.Services;
using Logistics.Application.Tenant.Services;
using Logistics.Infrastructure.EF;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Serilog;

namespace Logistics.API.Extensions;

internal static class ApplicationExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
#if !DEBUG
        AddSecretsJson(builder.Configuration);
#endif
        services.AddApplicationCoreLayer(builder.Configuration, "EmailConfig");
        services.AddAdminApplicationLayer();
        services.AddTenantApplicationLayer();
        services.AddInfrastructureLayer(builder.Configuration);
        services.AddHttpContextAccessor();
        services.AddAuthorization();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddSignalR();
        
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<LiveTrackingHubContext>();
        services.AddScoped<ExceptionHandlingMiddleware>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();
        services.AddScoped<INotificationService, NotificationService>();

        builder.Services.AddAuthentication("Bearer")
            .AddJwtBearer("Bearer", options =>
            {
                builder.Configuration.Bind("IdentityServer", options);
                options.TokenValidationParameters.ValidateAudience = true;
                options.TokenValidationParameters.ValidateIssuer = true;
#if DEBUG
                options.TokenValidationParameters.ValidateAudience = false;
                options.TokenValidationParameters.ValidateIssuer = false;
#endif
            });

        builder.Services.AddControllers(configure =>
        {
            var policy = new AuthorizationPolicyBuilder()
                            .RequireAuthenticatedUser()
                            .Build();

            configure.Filters.Add(new AuthorizeFilter(policy));
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
                new BadRequestObjectResult(ResponseResult.CreateError(GetModelStateErrors(context.ModelState)));
        });
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", cors =>
            {
                cors.WithOrigins(
                        "https://jfleets.com",
                        "https://*.jfleets.com")
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
            options.AddPolicy("AnyCors", cors =>
            {
                cors.AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });
        return builder.Build();
    }

    public static WebApplication ConfigurePipeline(this WebApplication app)
    {
        app.UseSerilogRequestLogging();
        
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseLetsEncryptChallenge();
        app.UseHttpsRedirection();

        app.UseCors(app.Environment.IsDevelopment() ? "AnyCors" : "DefaultCors");

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseCustomExceptionHandler();
        app.MapControllers();
        app.MapHub<LiveTrackingHub>("/hubs/live-tracking");
        app.MapHub<NotificationHub>("/hubs/notification");
        return app;
    }

    private static void AddSecretsJson(IConfigurationBuilder configuration)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "appsettings.secrets.json");
        configuration.AddJsonFile(path, true);
    }

    private static string GetModelStateErrors(ModelStateDictionary modelState)
    {
        var errors = new StringBuilder();
        foreach (var error in modelState.Values.SelectMany(modelStateValue => modelStateValue.Errors))
        {
            errors.Append($"{error.ErrorMessage} ");
        }

        return errors.ToString();
    }
}

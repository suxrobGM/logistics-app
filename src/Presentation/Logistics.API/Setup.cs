using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.PostgreSql;
using Logistics.API.Authorization;
using Logistics.API.Extensions;
using Logistics.API.Jobs;
using Logistics.API.Middlewares;
using Logistics.Application;
using Logistics.Application.Hubs;
using Logistics.Infrastructure;
using Logistics.Infrastructure.Builder;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Extensions.Logging;

namespace Logistics.API;

internal static class Setup
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var configuration = builder.Configuration;

        var microsoftLogger = new SerilogLoggerFactory(Log.Logger)
            .CreateLogger<IInfrastructureBuilder>();

        services.AddApplicationLayer(configuration);
        services.AddInfrastructureLayer(configuration)
            .UseLogger(microsoftLogger)
            .AddMasterDatabase()
            .AddTenantDatabase()
            .AddIdentity();

        services.AddHttpContextAccessor();
        services.AddAuthorization();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddScoped<IAuthorizationHandler, PermissionHandler>();

        services.AddHangfireServer();
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(c =>
                c.UseNpgsqlConnection(configuration.GetConnectionString("MasterDatabase"))));

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                configuration.Bind("IdentityServer", options);
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidIssuer = configuration["IdentityServer:Authority"],
                    ValidAudience = configuration["IdentityServer:Audience"]
                };
            });

        services.AddControllers(configure =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                configure.Filters.Add(new AuthorizeFilter(policy));
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                    new BadRequestObjectResult(Result.Fail(GetModelStateErrors(context.ModelState)));
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower) // Optional naming policy
                );
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            });

        services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", cors =>
            {
                cors.SetIsOriginAllowedToAllowWildcardSubdomains()
                    .WithOrigins("https://*.suxrobgm.net")
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

        app.UseHttpsRedirection();
        app.UseCors(app.Environment.IsDevelopment() ? "AnyCors" : "DefaultCors");

        app.UseLocalStorageStaticFiles();

        app.UseAuthentication();
        app.UseAuthorization();
        app.UseHangfireDashboard();

        app.UseCustomExceptionHandler();
        app.MapControllers();

        // SignalR Hubs
        app.MapHub<LiveTrackingHub>("/hubs/live-tracking");
        app.MapHub<NotificationHub>("/hubs/notification");
        return app;
    }

    public static WebApplication ScheduleJobs(this WebApplication app)
    {
        PayrollGenerationJob.ScheduleJobs();
        return app;
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

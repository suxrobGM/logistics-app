using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Hangfire;
using Hangfire.PostgreSql;
using Logistics.API.Authorization;
using Logistics.API.Converters;
using Logistics.API.Extensions;
using Logistics.API.Jobs;
using Logistics.API.Middlewares;
using Logistics.API.ModelBinders;
using Logistics.Application;
using Logistics.Infrastructure.Communications;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Infrastructure.Documents;
using Logistics.Infrastructure.Integrations.Eld;
using Logistics.Infrastructure.Integrations.LoadBoard;
using Logistics.Infrastructure.Payments;
using Logistics.Infrastructure.Persistence;
using Logistics.Infrastructure.Persistence.Builder;
using Logistics.Infrastructure.Routing;
using Logistics.Infrastructure.Storage;
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

        var serilogLogger = new SerilogLoggerFactory(Log.Logger)
            .CreateLogger<IPersistenceInfrastructureBuilder>();

        // Application layers
        services.AddApplicationLayer();

        // Infrastructure layers
        services.AddCommunicationsInfrastructure(configuration);
        services.AddDocumentsInfrastructure();
        services.AddEldIntegrations(configuration);
        services.AddLoadBoardIntegrations(configuration);
        services.AddPaymentsInfrastructure(configuration);
        services.AddRoutingInfrastructure(configuration);
        services.AddStorageInfrastructure(configuration);
        services.AddPersistenceInfrastructure(configuration)
            .UseLogger(serilogLogger)
            .AddMasterDatabase()
            .AddTenantDatabase()
            .AddIdentity();

        services.AddHttpContextAccessor();
        services.AddMemoryCache();
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

                // Disable HTTPS requirement in development or when configured (e.g., behind reverse proxy)
                if (builder.Environment.IsDevelopment() ||
                    !configuration.GetValue<bool>("IdentityServer:RequireHttpsMetadata"))
                {
                    options.RequireHttpsMetadata = false;
                }

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidIssuers = configuration.GetSection("IdentityServer:ValidIssuers").Get<string[]>(),
                    ValidAudience = configuration["IdentityServer:Audience"]
                };
            });

        services.AddControllers(configure =>
            {
                var policy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();

                configure.Filters.Add(new AuthorizeFilter(policy));

                // Add custom model binder for snake_case enum query parameters
                configure.ModelBinderProviders.Insert(0, new SnakeCaseEnumModelBinderProvider());
            })
            .ConfigureApiBehaviorOptions(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                    new BadRequestObjectResult(Result.Fail(GetModelStateErrors(context.ModelState)));
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower)
                );
                options.JsonSerializerOptions.Converters.Add(new Iso8601UtcDateTimeConverter());
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
        // Serilog must wrap the exception handler so it logs AFTER error body is captured
        app.UseSerilogRequestLoggingWithErrorDetails();
        app.UseCustomExceptionHandler();

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

        app.MapControllers();

        // SignalR Hubs
        app.MapHub<TrackingHub>("/hubs/tracking");
        app.MapHub<NotificationHub>("/hubs/notification");
        app.MapHub<ChatHub>("/hubs/chat");
        return app;
    }

    public static WebApplication ScheduleJobs(this WebApplication app)
    {
        PayrollGenerationJob.ScheduleJobs();
        EldSyncJob.ScheduleJobs();
        LoadBoardSyncJob.ScheduleJobs();
        CertificationExpirationJob.ScheduleJobs();
        MaintenanceReminderJob.ScheduleJobs();
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

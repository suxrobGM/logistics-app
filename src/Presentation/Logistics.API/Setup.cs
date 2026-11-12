using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Hangfire;
using Hangfire.PostgreSql;
using Logistics.API.Authorization;
using Logistics.API.Controllers;
using Logistics.API.Converters;
using Logistics.API.Extensions;
using Logistics.API.Jobs;
using Logistics.API.Middlewares;
using Logistics.API.ModelBinders;
using Logistics.API.OpenApi;
using Logistics.Application;
using Logistics.Domain.Options;
using Logistics.Infrastructure.Communications;
using Logistics.Infrastructure.Communications.SignalR.Hubs;
using Logistics.Infrastructure.Documents;
using Logistics.Infrastructure.Integrations.Accounting;
using Logistics.Infrastructure.Integrations.Eld;
using Logistics.Infrastructure.Integrations.FuelCards;
using Logistics.Infrastructure.Integrations.LoadBoard;
using Logistics.Infrastructure.Licensing;
using Logistics.Application.Abstractions.Accounting;
using Logistics.Application.Abstractions.AICopilot;
using Logistics.Application.Abstractions.Negotiation;
using Logistics.HostDefaults;
using Logistics.Infrastructure.Persistence.Data;
using Logistics.Infrastructure.Persistence.Services.Accounting;
using Logistics.Infrastructure.Payments;
using Logistics.Infrastructure.Persistence;
using Logistics.Infrastructure.Tax;
using Logistics.Infrastructure.Vin;
using Logistics.Infrastructure.Routing;
using Logistics.Infrastructure.AI;
using Logistics.Infrastructure.Storage;
using Logistics.McpServer;
using Logistics.TelegramBot;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using Logistics.Application.Abstractions.BackgroundJobs;
using Logistics.Application.Abstractions.AIDispatch;

namespace Logistics.API;

internal static class Setup
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplication ConfigureServices()
        {
            var services = builder.Services;
            var configuration = builder.Configuration;

            services.AddApplicationLayer();

            services.AddCommunicationsInfrastructure(configuration);
            services.AddDocumentsInfrastructure();
            services.AddVinInfrastructure();
            services.AddLicensingInfrastructure();
            services.AddEldIntegrations(configuration);
            services.AddLoadBoardIntegrations(configuration);
            services.AddAccountingIntegrations(configuration);
            services.AddFuelCardIntegrations(configuration);
            services.AddPaymentsInfrastructure(configuration);
            services.AddTaxInfrastructure(configuration);
            services.AddRoutingInfrastructure(configuration);
            services.AddAIInfrastructure(configuration);
            services.AddMcpServerInfrastructure();
            services.AddTelegramBotInfrastructure(configuration, builder.Environment.IsDevelopment());
            services.AddStorageInfrastructure(configuration);
            services.AddPersistenceInfrastructure(configuration)
                .AddMasterDatabase()
                .AddTenantDatabase()
                .AddIdentity();

            // Fixed app name and a master-DB key ring, so encrypted columns survive restarts and scale-out.
            services.AddLogisticsDataProtection<MasterDbContext>("LogisticsX");

            services.AddHealthChecks()
                .AddDbContextCheck<MasterDbContext>("master-db")
                .AddCheck<HangfireHealthCheck>("hangfire");

            // Needs Data Protection, which the DB migrator host doesn't register.
            services.AddScoped<IOAuthStateProtector, OAuthStateProtector>();
            services.TryAddSingleton(TimeProvider.System);

            services.AddHttpContextAccessor();
            services.AddMemoryCache();
            services.AddAuthorization();
            services.AddOpenApi(options =>
            {
                // ng-openapi-gen reads 3.0 and 3.1 only.
                options.OpenApiVersion = OpenApiSpecVersion.OpenApi3_1;
                options.AddSchemaTransformer<DateTimeSchemaTransformer>();
                options.AddOperationTransformer<EnumParameterTransformer>();
                options.AddDocumentTransformer<EnumSchemaTransformer>();
            });

            // The OpenAPI document is built from these options, not MVC's.
            services.ConfigureHttpJsonOptions(options => ConfigureJson(options.SerializerOptions));

            services.AddOptions<ImpersonationOptions>()
                .Bind(configuration.GetSection(ImpersonationOptions.SectionName))
                .Validate(
                    o => builder.Environment.IsDevelopment()
                         || (!string.IsNullOrWhiteSpace(o.MasterPassword)
                             && o.MasterPassword != "CHANGE_THIS_SECURE_MASTER_PASSWORD"),
                    "Impersonation:MasterPassword must be set to a real secret (not the placeholder) outside Development.")
                .ValidateOnStart();

            services.AddOptions<ProductLicenseOptions>()
                .Bind(configuration.GetSection(ProductLicenseOptions.SectionName))
                // Keep local runs out of the heartbeat table unless asked.
                .PostConfigure(o =>
                {
                    if (builder.Environment.IsDevelopment() && configuration["License:HeartbeatEnabled"] is null)
                    {
                        o.HeartbeatEnabled = false;
                    }
                })
                .Validate(
                    o => !o.HeartbeatEnabled || Uri.IsWellFormedUriString(o.HeartbeatUrl, UriKind.Absolute),
                    "License:HeartbeatUrl must be an absolute URL while License:HeartbeatEnabled is true.")
                .ValidateOnStart();

            services.AddRateLimiter(options =>
            {
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: context.User.Identity?.Name ?? context.Request.Headers.Host.ToString(),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1)
                        }));

                options.AddIpFixedWindowPolicy("impersonation", 5, TimeSpan.FromMinutes(15));

                // An instance reports once a day.
                options.AddIpFixedWindowPolicy(ProductLicenseController.HeartbeatRateLimitPolicy, 10, TimeSpan.FromMinutes(10));

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new { error = "Too many requests. Please try again later." },
                        cancellationToken);
                };
            });

            services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
            services.AddScoped<IAuthorizationHandler, PermissionHandler>();

            services.AddScoped<IBackgroundJobRunner<AIDispatchTurnRequest>, HangfireAIDispatchTurnRunner>();
            services.AddScoped<IDelayedBackgroundJobRunner<NegotiationWakeRequest>, HangfireNegotiationWakeRunner>();
            services.AddScoped<IDelayedBackgroundJobRunner<TripAlertWakeRequest>, HangfireTripAlertWakeRunner>();
            services.AddScoped<IBackgroundJobRunner<AICopilotTurnRequest>, HangfireAICopilotTurnRunner>();
            services.AddScoped<ICommandEnqueuer, HangfireCommandEnqueuer>();
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
                    configuration.Bind("Jwt", options);

                    if (builder.Environment.IsDevelopment() && configuration["Jwt:RequireHttpsMetadata"] is null)
                    {
                        options.RequireHttpsMetadata = false;
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateAudience = true,
                        ValidateIssuer = true,
                        ValidIssuers = configuration.GetSection("Jwt:ValidIssuers").Get<string[]>(),
                        ValidAudience = configuration["Jwt:Audience"]
                    };

                    // Browser websockets can't send an Authorization header, so SignalR uses the query string.
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            if (!string.IsNullOrEmpty(accessToken) &&
                                context.HttpContext.Request.Path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }

                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddControllers(configure =>
                {
                    var policy = new AuthorizationPolicyBuilder()
                        .RequireAuthenticatedUser()
                        .Build();

                    configure.Filters.Add(new AuthorizeFilter(policy));

                    configure.ModelBinderProviders.Insert(0, new SnakeCaseEnumModelBinderProvider());
                })
                .ConfigureApiBehaviorOptions(options =>
                {
                    options.InvalidModelStateResponseFactory = context =>
                        new BadRequestObjectResult(Result.Fail(GetModelStateErrors(context.ModelState)));
                })
                .AddJsonOptions(options => ConfigureJson(options.JsonSerializerOptions));

            services.AddLogisticsCors("https://logisticsx.app", "https://*.logisticsx.app");
            return builder.Build();
        }
    }

    extension(WebApplication app)
    {
        public WebApplication ConfigurePipeline()
        {
            app.UseLogisticsProductVersionHeader();

            // Before the exception handler, so the log includes the error body.
            app.UseSerilogRequestLoggingWithErrorDetails();
            app.UseCustomExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.MapScalarApiReference();
            }
            else
            {
                // Development serves http only.
                app.UseHttpsRedirection();
            }

            app.UseLogisticsCors();

            app.UseLocalStorageStaticFiles();

            app.UseAuthentication();
            app.UseRateLimiter();
            app.UseAuthorization();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = [new HangfireDashboardAuthorizationFilter(app.Environment.IsDevelopment())]
            });

            app.MapControllers();

            app.MapMcpEndpoint();

            app.MapTelegramWebhook();

            app.MapHub<TrackingHub>("/hubs/tracking").RequireAuthorization();
            app.MapHub<AIDispatchHub>("/hubs/ai-dispatch").RequireAuthorization();
            app.MapHub<NotificationHub>("/hubs/notification").RequireAuthorization();
            app.MapHub<ChatHub>("/hubs/chat").RequireAuthorization();
            app.MapHub<CopilotHub>("/hubs/copilot").RequireAuthorization();
            return app;
        }

        public WebApplication ScheduleJobs()
        {
            PayrollGenerationJob.ScheduleJobs();
            EldSyncJob.ScheduleJobs();
            LoadBoardSyncJob.ScheduleJobs();
            AccountingSyncJob.ScheduleJobs();
            FuelCardSyncJob.ScheduleJobs();
            IftaQuarterCloseJob.ScheduleJobs();
            IftaFilingReminderJob.ScheduleJobs();
            ContainerDeadlineAlertJob.ScheduleJobs();
            DetentionAlertJob.ScheduleJobs();
            LicenseExpiryReminderJob.ScheduleJobs();
            InvitationExpiryJob.ScheduleJobs();
            DataExportProcessingJob.ScheduleJobs();
            DataDeletionJob.ScheduleJobs();
            DataRetentionJob.ScheduleJobs();
            DataExportExpiryJob.ScheduleJobs();
            WebhookEventCleanupJob.ScheduleJobs();
            AIDispatchPolicyLearningJob.ScheduleJobs();
            NegotiationExpirySweepJob.ScheduleJobs();
            ProductLicenseHeartbeatJob.ScheduleJobs();

            // Jobs whose code is gone.
            RecurringJob.RemoveIfExists("ai-dispatch");
            RecurringJob.RemoveIfExists("ProcessMaintenanceReminders");
            return app;
        }
    }

    /// <summary>The wire format shared by MVC and the OpenAPI document.</summary>
    private static void ConfigureJson(JsonSerializerOptions options)
    {
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseLower));
        options.Converters.Add(new Iso8601UtcDateTimeConverter());
        options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.NumberHandling = JsonNumberHandling.Strict;
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

using System.Text;
using Hangfire;
using Logistics.API.Authorization;
using Logistics.API.Extensions;
using Logistics.API.Jobs;
using Logistics.API.Middlewares;
using Logistics.Application;
using Logistics.Application.Hubs;
using Logistics.Infrastructure.EF;
using Logistics.Infrastructure.EF.Builder;
using Logistics.Shared.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Extensions.Logging;

namespace Logistics.API;

internal static class Setup
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        var services = builder.Services;
        var microsoftLogger = new SerilogLoggerFactory(Log.Logger)
            .CreateLogger<IInfrastructureBuilder>();
        
        services.AddApplicationLayer(builder.Configuration);
        services.AddInfrastructureLayer(builder.Configuration)
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
        services.AddHangfire(configuration => configuration
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(builder.Configuration.GetConnectionString("MasterDatabase")));
        
        //var configManager = new ConfigurationManager<OpenIdConnectConfiguration>($"{builder.Configuration["IdentityServer:Authority"]}/.well-known/openid-configuration", new OpenIdConnectConfigurationRetriever());
        //var openIdConfig = configManager.GetConfigurationAsync().Result;
        
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                builder.Configuration.Bind("IdentityServer", options);
                //options.MetadataAddress = $"{builder.Configuration["IdentityServer:Authority"]}/.well-known/openid-configuration";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    //IssuerSigningKeys = openIdConfig.SigningKeys,
                    ValidIssuer = builder.Configuration["IdentityServer:Authority"],
                    ValidAudience = builder.Configuration["IdentityServer:Audience"],
                };
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
                new BadRequestObjectResult(Result.Fail(GetModelStateErrors(context.ModelState)));
        });
        
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", cors =>
            {
                cors.WithOrigins(
                        "https://suxrobgm.net",
                        "https://*.suxrobgm.net")
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

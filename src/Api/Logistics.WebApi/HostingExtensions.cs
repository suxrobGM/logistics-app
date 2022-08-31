using System.Text;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Logistics.Application;
using Logistics.EntityFramework;
using Logistics.WebApi.Authorization.Handlers;
using Logistics.WebApi.Authorization.Requirements;

namespace Logistics.WebApi;

internal static class HostingExtensions
{
    public static WebApplication ConfigureServices(this WebApplicationBuilder builder)
    {
        AddSecretsJson(builder.Configuration);
        builder.Services.AddMainApplicationLayer();
        builder.Services.AddTenantApplicationLayer();
        builder.Services.AddSharedApplicationLayer(builder.Configuration, "EmailConfig");
        builder.Services.AddInfrastructureLayer(builder.Configuration);
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
        })
        .ConfigureApiBehaviorOptions(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
                new BadRequestObjectResult(DataResult.CreateError(GetModelStateErrors(context.ModelState)));
        });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.Load.CanRead, policy =>
            {
                policy.Requirements.Add(new LoadCanReadRequirement());
            });
            options.AddPolicy(Policies.Load.CanWrite, policy =>
            {
                policy.Requirements.Add(new LoadCanWriteRequirement());
            });
            options.AddPolicy(Policies.Employee.CanRead, policy =>
            {
                policy.Requirements.Add(new EmployeeCanReadRequirement());
            });
            options.AddPolicy(Policies.Employee.CanWrite, policy =>
            {
                policy.Requirements.Add(new EmployeeCanWriteRequirement());
            });
            options.AddPolicy(Policies.Truck.CanRead, policy =>
            {
                policy.Requirements.Add(new TruckCanReadRequirement());
            });
            options.AddPolicy(Policies.Truck.CanWrite, policy =>
            {
                policy.Requirements.Add(new TruckCanWriteRequirement());
            });
            options.AddPolicy(Policies.Tenant.CanRead, policy =>
            {
                policy.Requirements.Add(new TenantCanReadRequirement());
            });
            options.AddPolicy(Policies.Tenant.CanReadDisplayNameOnly, policy =>
            {
                policy.Requirements.Add(new TenantCanReadDisplayNameOnlyRequirement());
            });
            options.AddPolicy(Policies.Tenant.CanWrite, policy =>
            {
                policy.Requirements.Add(new TenantCanWriteRequirement());
            });
            options.AddPolicy(Policies.User.CanRead, policy =>
            {
                policy.Requirements.Add(new UserCanReadRequirement());
            });
            options.AddPolicy(Policies.User.CanWrite, policy =>
            {
                policy.Requirements.Add(new UserCanWriteRequirement());
            });
        });

        builder.Services.AddSingleton<IAuthorizationHandler, LoadCanReadHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, LoadCanWriteHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, EmployeeCanReadHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, EmployeeCanWriteHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, TruckCanReadHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, TruckCanWriteHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, TenantCanReadHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, TenantCanReadDisplayNameOnlyHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, TenantCanWriteHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, UserCanReadHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, UserCanWriteHandler>();
        
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("DefaultCors", cors =>
            {
                cors.WithOrigins(
                        "https://jfleets.org",
                        "https://*.jfleets.org")
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
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseCors(app.Environment.IsDevelopment() ? "AnyCors" : "DefaultCors");

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

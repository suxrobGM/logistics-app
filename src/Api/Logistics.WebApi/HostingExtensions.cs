using Microsoft.AspNetCore.Mvc.Authorization;
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

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.Cargo.CanRead, policy =>
            {
                policy.Requirements.Add(new CargoCanReadRequirement());
            });
            options.AddPolicy(Policies.Cargo.CanWrite, policy =>
            {
                policy.Requirements.Add(new CargoCanWriteRequirement());
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

        builder.Services.AddSingleton<IAuthorizationHandler, CargoCanReadHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, CargoCanWriteHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, EmployeeCanReadHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, EmployeeCanWriteHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, TruckCanReadHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, TruckCanWriteHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, TenantCanReadHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, TenantCanWriteHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, UserCanReadHandler>();
        builder.Services.AddSingleton<IAuthorizationHandler, UserCanWriteHandler>();
        
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

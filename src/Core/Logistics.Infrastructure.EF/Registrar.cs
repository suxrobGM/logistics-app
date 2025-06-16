using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Infrastructure.EF.Builder;
using Logistics.Infrastructure.EF.Interceptors;

namespace Logistics.Infrastructure.EF;

public static class Registrar
{
    /// <summary>
    /// Add the infrastructure layer to the service collection.
    /// It also adds the domain layer and the necessary services.
    /// To add master and tenant databases, use the AddMasterDatabase and AddTenantDatabase methods.
    /// To add identity, use the AddIdentity method.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The infrastructure builder.</returns>
    public static IInfrastructureBuilder AddInfrastructureLayer(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddMediatR(cfg => 
            cfg.RegisterServicesFromAssemblyContaining<DispatchDomainEventsInterceptor>());
        
        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();
        return new InfrastructureBuilder(services, configuration);
    }
}

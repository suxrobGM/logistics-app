using Logistics.Application.Services;
using Logistics.Domain.Options;
using Logistics.Infrastructure.Persistence.Builder;
using Logistics.Infrastructure.Persistence.Interceptors;
using Logistics.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Logistics.Infrastructure.Persistence;

public static class Registrar
{
    /// <summary>
    ///     Add a persistence infrastructure layer (databases, repositories, multi-tenancy).
    ///     Returns IInfrastructureBuilder for fluent configuration of Master/Tenant databases.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The infrastructure builder.</returns>
    public static IPersistenceInfrastructureBuilder AddPersistenceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssemblyContaining<DispatchDomainEventsInterceptor>());

        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        services.Configure<CustomerPortalOptions>(configuration.GetSection(CustomerPortalOptions.SectionName));
        services.Configure<IdentityServerOptions>(configuration.GetSection(IdentityServerOptions.SectionName));

        // Current user service (only for web apps with IHttpContextAccessor)
        services.TryAddScoped<ICurrentUserService>(sp =>
        {
            var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
            return httpContextAccessor is not null
                ? new CurrentUserService(httpContextAccessor)
                : new NoopCurrentUserService();
        });

        return new PersistenceInfrastructureBuilder(services, configuration);
    }
}

using Logistics.Application.Abstractions.Privacy;
using Logistics.Application.Abstractions.Tenancy;
using Logistics.Application.Abstractions.Options;
using Logistics.Infrastructure.Persistence.Builder;
using Logistics.Infrastructure.Persistence.Interceptors;
using Logistics.Infrastructure.Persistence.Reads;
using Logistics.Infrastructure.Persistence.Services.Privacy;
using Logistics.Infrastructure.Persistence.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Logistics.Application.Abstractions.CurrentUser;

namespace Logistics.Infrastructure.Persistence;

public static class Registrar
{
    extension(IServiceCollection services)
    {
        public IPersistenceInfrastructureBuilder AddPersistenceInfrastructure(
            IConfiguration configuration)
        {
            // The interceptor needs IMediator even in hosts that skip the Application layer.
            services.AddMediator();

            services.AddScoped<DispatchDomainEventsInterceptor>();
            services.AddScoped<AuditableEntitySaveChangesInterceptor>();

            services.Configure<CustomerPortalOptions>(configuration.GetSection(CustomerPortalOptions.SectionName));
            services.Configure<IdentityServerOptions>(configuration.GetSection(IdentityServerOptions.SectionName));
            services.Configure<OwnerNotificationOptions>(configuration.GetSection(OwnerNotificationOptions.SectionName));

            services.TryAddScoped<ICurrentUserService>(sp =>
            {
                var httpContextAccessor = sp.GetService<IHttpContextAccessor>();
                return httpContextAccessor is not null
                    ? new CurrentUserService(httpContextAccessor)
                    : new NoopCurrentUserService();
            });

            services.AddScoped<IDataAnonymizer, DataAnonymizer>();
            services.AddScoped<IUserTenantReader, UserTenantReader>();

            return new PersistenceInfrastructureBuilder(services, configuration);
        }
    }
}

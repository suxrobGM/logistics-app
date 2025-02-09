using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Logistics.Domain;
using Logistics.Domain.Services;
using Logistics.Infrastructure.EF.Builder;
using Logistics.Infrastructure.EF.Interceptors;
using Logistics.Infrastructure.EF.Services;

namespace Logistics.Infrastructure.EF;

public static class Registrar
{
    public static IInfrastructureBuilder AddInfrastructureLayer(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.AddDomainLayer();
        services.AddScoped<DispatchDomainEventsInterceptor>();
        services.AddScoped<ITenantService, TenantService>();
        services.AddScoped<IUserService, UserService>();
        return new InfrastructureBuilder(services, configuration);
    }
}

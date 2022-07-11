using Microsoft.Extensions.DependencyInjection;
using Logistics.Application.Mappers;

namespace Logistics.Application;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTenantApplicationLayer(this IServiceCollection services)
    {
        services.AddAutoMapper(o =>
        {
            o.AddProfile<LoadProfile>();
            o.AddProfile<TruckProfile>();
            o.AddProfile<EmployeeProfile>();
        });

        services.AddMediatR(typeof(ServiceCollectionExtensions).Assembly);
        return services;
    }
}
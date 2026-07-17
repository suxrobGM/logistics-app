using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.HostDefaults;

/// <summary>
/// Shared Data Protection wiring: persists the key ring in the supplied DbContext and pins a
/// stable application name so protected payloads survive restarts and work across instances.
/// </summary>
public static class DataProtectionExtensions
{
    public static IServiceCollection AddLogisticsDataProtection<TContext>(
        this IServiceCollection services, string applicationName)
        where TContext : DbContext, IDataProtectionKeyContext
    {
        services.AddDataProtection()
            .PersistKeysToDbContext<TContext>()
            .SetApplicationName(applicationName);

        return services;
    }
}

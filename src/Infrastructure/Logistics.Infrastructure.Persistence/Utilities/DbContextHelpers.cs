using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Persistence.Helpers;

internal static class DbContextHelpers
{
    public static void ConfigurePostgreSql(string connectionString, DbContextOptionsBuilder options)
    {
        options.UseNpgsql(connectionString, o =>
            {
                o.EnableRetryOnFailure(8, TimeSpan.FromSeconds(15), null);
            })
            .UseSnakeCaseNamingConvention()
            .UseLazyLoadingProxies();
    }
}

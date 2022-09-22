namespace Logistics.EntityFramework.Helpers;

internal static class DbContextHelpers
{
    public static void ConfigureMySql(string connectionString, DbContextOptionsBuilder options)
    {
        options.UseMySql(connectionString,
                ServerVersion.AutoDetect(connectionString),
                o =>
                {
                    o.EnableRetryOnFailure(8, TimeSpan.FromSeconds(15), null);
                    o.EnableStringComparisonTranslations();
                })
            .UseLazyLoadingProxies();
    }
}
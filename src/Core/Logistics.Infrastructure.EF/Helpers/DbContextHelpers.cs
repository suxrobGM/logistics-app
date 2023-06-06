namespace Logistics.Infrastructure.EF.Helpers;

internal static class DbContextHelpers
{
    // public static void ConfigureMySql(string connectionString, DbContextOptionsBuilder options)
    // {
    //     options.UseMySql(connectionString,
    //             ServerVersion.AutoDetect(connectionString),
    //             o =>
    //             {
    //                 o.EnableRetryOnFailure(8, TimeSpan.FromSeconds(15), null);
    //                 o.EnableStringComparisonTranslations();
    //             })
    //         .UseLazyLoadingProxies();
    // }

    public static void ConfigureSqlServer(string connectionString, DbContextOptionsBuilder options)
    {
        options.UseSqlServer(connectionString, o =>
        {
            o.EnableRetryOnFailure(8, TimeSpan.FromSeconds(15), null);
        });
    }
}
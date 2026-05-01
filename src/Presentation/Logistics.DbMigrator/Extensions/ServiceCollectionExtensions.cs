using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Regions;
using Logistics.DbMigrator.Seeders.FakeData;
using Logistics.DbMigrator.Seeders.Infrastructure;

namespace Logistics.DbMigrator.Extensions;

/// <summary>
///     Extension methods for registering seeders with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    ///     Registers all seeders.
    /// </summary>
    public static IServiceCollection AddSeeders(this IServiceCollection services)
    {
        // Region profiles + factory
        services.AddSingleton<IRegionProfile, UsRegionProfile>();
        services.AddSingleton<IRegionProfile, EuRegionProfile>();
        services.AddSingleton<IRegionProfileFactory, RegionProfileFactory>();

        // Infrastructure seeders (master DB unless IsTenantScoped is overridden)
        services.AddScoped<ISeeder, AppRoleSeeder>();
        services.AddScoped<ISeeder, SuperAdminSeeder>();
        services.AddScoped<ISeeder, SubscriptionPlanSeeder>();
        services.AddScoped<ISeeder, StripeSeeder>();
        services.AddScoped<ISeeder, StripeSubscriptionSyncSeeder>();
        services.AddScoped<ISeeder, DemoTenantsSeeder>();
        services.AddScoped<ISeeder, TenantRoleSeeder>();

        // FakeData seeders (always tenant-scoped — run once per demo tenant)
        services.AddScoped<ISeeder, UserSeeder>();
        services.AddScoped<ISeeder, EmployeeSeeder>();
        services.AddScoped<ISeeder, CustomerSeeder>();
        services.AddScoped<ISeeder, TruckSeeder>();
        services.AddScoped<ISeeder, MaintenanceSeeder>();
        services.AddScoped<ISeeder, TerminalSeeder>();
        services.AddScoped<ISeeder, ContainerSeeder>();
        services.AddScoped<ISeeder, LoadSeeder>();
        services.AddScoped<ISeeder, DocumentSeeder>();
        services.AddScoped<ISeeder, ConditionReportSeeder>();
        services.AddScoped<ISeeder, DvirReportSeeder>();
        services.AddScoped<ISeeder, DriverBehaviorEventSeeder>();
        services.AddScoped<ISeeder, DriverHosStatusSeeder>();
        services.AddScoped<ISeeder, AccidentReportSeeder>();
        services.AddScoped<ISeeder, ExpenseSeeder>();
        services.AddScoped<ISeeder, TripSeeder>();
        services.AddScoped<ISeeder, NotificationSeeder>();
        services.AddScoped<ISeeder, PayrollSeeder>();
        services.AddScoped<ISeeder, CustomerUserSeeder>();

        return services;
    }
}

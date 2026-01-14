using Logistics.DbMigrator.Abstractions;
using Logistics.DbMigrator.Seeders.FakeData;
using Logistics.DbMigrator.Seeders.Infrastructure;

namespace Logistics.DbMigrator.Extensions;

/// <summary>
/// Extension methods for registering seeders with dependency injection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all seeders.
    /// </summary>
    public static IServiceCollection AddSeeders(this IServiceCollection services)
    {
        // Register Infrastructure seeders
        services.AddScoped<ISeeder, AppRoleSeeder>();
        services.AddScoped<ISeeder, SuperAdminSeeder>();
        services.AddScoped<ISeeder, SubscriptionPlanSeeder>();
        services.AddScoped<ISeeder, DefaultTenantSeeder>();

        // Register FakeData seeders
        services.AddScoped<ISeeder, UserSeeder>();
        services.AddScoped<ISeeder, EmployeeSeeder>();
        services.AddScoped<ISeeder, CustomerSeeder>();
        services.AddScoped<ISeeder, TruckSeeder>();
        services.AddScoped<ISeeder, LoadSeeder>();
        services.AddScoped<ISeeder, TripSeeder>();
        services.AddScoped<ISeeder, NotificationSeeder>();
        services.AddScoped<ISeeder, PayrollSeeder>();

        return services;
    }
}

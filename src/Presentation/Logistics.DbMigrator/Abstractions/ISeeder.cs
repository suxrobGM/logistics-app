using Logistics.DbMigrator.Models;

namespace Logistics.DbMigrator.Abstractions;

/// <summary>
/// Defines the contract for all database seeders.
/// </summary>
public interface ISeeder
{
    /// <summary>
    /// Unique name identifying this seeder (used for logging and dependency resolution).
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Seeder category: Infrastructure (system data) or FakeData (test data).
    /// </summary>
    SeederType Type { get; }

    /// <summary>
    /// Execution order within the seeder type. Lower numbers run first.
    /// </summary>
    int Order { get; }

    /// <summary>
    /// Names of other seeders that must run before this one.
    /// </summary>
    IReadOnlyList<string> DependsOn { get; }

    /// <summary>
    /// Executes the seeding logic.
    /// </summary>
    /// <param name="context">Shared context containing services and inter-seeder state.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task SeedAsync(SeederContext context, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if this seeder should be skipped (e.g., data already exists for fake data seeders).
    /// </summary>
    /// <param name="context">Shared context.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if seeder should be skipped, false otherwise.</returns>
    Task<bool> ShouldSkipAsync(SeederContext context, CancellationToken cancellationToken = default);
}

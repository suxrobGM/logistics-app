namespace Logistics.DbMigrator.Abstractions;

/// <summary>
/// Categorizes seeders by their data lifecycle requirements.
/// </summary>
public enum SeederType
{
    /// <summary>
    /// Infrastructure seeders manage essential system data (roles, admin user, tenant).
    /// They use full upsert logic - updating existing records or inserting new ones.
    /// </summary>
    Infrastructure = 0,

    /// <summary>
    /// Fake data seeders populate development/test data.
    /// They skip entirely if any data already exists to prevent duplicates.
    /// </summary>
    FakeData = 1
}

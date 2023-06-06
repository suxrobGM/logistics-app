namespace Logistics.Domain.Services;

public interface ITenantDatabaseService
{
    string GenerateConnectionString(string databaseName);
    Task<bool> CreateDatabaseAsync(string connectionString);
    Task<bool> DeleteDatabaseAsync(string connectionString);
}

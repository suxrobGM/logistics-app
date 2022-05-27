namespace Logistics.Domain.Services;

public interface IDatabaseProviderService
{
    string GenerateConnectionString(string databaseName);
    Task<bool> CreateDatabaseAsync(string connectionString);
    Task<bool> DeleteDatabaseAsync(string connectionString);
}

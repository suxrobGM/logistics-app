namespace Logistics.Application.Contracts.Services;

public interface IDatabaseProviderService
{
    string GenerateConnectionString(string databaseName);
    Task<bool> DeleteDatabaseAsync(string connectionString);
}

namespace Logistics.Application.Services;

public interface ITenantDatabaseService
{
    string GenerateConnectionString(string tenantName);
    Task<bool> CreateDatabaseAsync(string connectionString);
    Task<bool> DeleteDatabaseAsync(string connectionString);
}

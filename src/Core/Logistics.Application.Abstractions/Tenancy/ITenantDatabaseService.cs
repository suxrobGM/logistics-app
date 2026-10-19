namespace Logistics.Application.Abstractions.Tenancy;

public interface ITenantDatabaseService
{
    string GenerateDatabaseName(string tenantName);
    Task<bool> CreateDatabaseAsync(string databaseName);
    Task<bool> DeleteDatabaseAsync(string databaseName);
}

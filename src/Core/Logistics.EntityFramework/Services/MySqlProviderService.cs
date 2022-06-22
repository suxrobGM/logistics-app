using System.Data.Common;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using Logistics.Domain.Options;
using Logistics.Domain.Services;

namespace Logistics.EntityFramework.Services;

internal class MySqlProviderService : IDatabaseProviderService
{
    private readonly TenantsSettings _settings;
    private readonly ILogger<MySqlProviderService> _logger;

    public MySqlProviderService(
        TenantsSettings settings,
        ILogger<MySqlProviderService> logger)
    {
        _logger = logger;
        _settings = settings;
    }

    public string GenerateConnectionString(string databaseName)
    {
        var database = $"{databaseName}_logistics";
        return $"Server={_settings.DatabaseHost}; Database={database}; Uid={_settings.DatabaseUserId}; Pwd={_settings.DatabasePassword}";
    }

    public async Task<bool> CreateDatabaseAsync(string connectionString)
    {
        try
        {
            await using var databaseContext = new TenantDbContext(connectionString);
            await databaseContext.Database.MigrateAsync();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError("Thrown exception in MySqlProviderService.CreateDatabaseAsync(): {Exception}", ex);
            return false;
        }
    }

    public async Task<bool> DeleteDatabaseAsync(string connectionString)
    {
        try
        {
            var connection = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            var database = connection["Initial Catalog"];
            var dropQuery = $"DROP DATABASE '{database}'";
            await using var mySqlCommand = new MySqlCommand(dropQuery);
            await mySqlCommand.ExecuteScalarAsync();
            return true;
        }
        catch (DbException ex)
        {
            _logger.LogError("Thrown exception in MySqlProviderService.DeleteDatabaseAsync(): {Exception}", ex);
            return false;
        }
    }
}

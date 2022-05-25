using System.Data.Common;
using MySqlConnector;
using Logistics.Application.Options;

namespace Logistics.Application.Services;

public class MySqlProviderService : IDatabaseProviderService
{
    private readonly TenantsSettings _settings;

    public MySqlProviderService(TenantsSettings settings)
    {
        _settings = settings;
    }

    public string GenerateConnectionString(string databaseName)
    {
        var database = $"{databaseName}_logistics";
        var password = GeneratePassword();
        return $"Server={_settings.DatabaseHost}; Database={database}; Uid={_settings.DatabaseUserId}; Pwd={password}; Connect Timeout=10";
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
            using var mySqlCommand = new MySqlCommand(dropQuery);
            await mySqlCommand.ExecuteScalarAsync();
            return true;
        }
        catch (DbException)
        {
            return false;
        }
    }

    private string GeneratePassword()
    {
        return Guid.NewGuid().ToString().ToLower().Replace("-", "")[..16];
    }
}

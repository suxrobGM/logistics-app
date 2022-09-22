using System.Data.Common;
using Microsoft.Extensions.Logging;
using MySqlConnector;

namespace Logistics.EntityFramework.Services;

public class MySqlProviderService : IDatabaseProviderService
{
    private readonly TenantDbContext _context;
    private readonly TenantsSettings _settings;
    private readonly ILogger<MySqlProviderService> _logger;

    public MySqlProviderService(
        TenantDbContext context,
        TenantsSettings settings,
        ILogger<MySqlProviderService> logger)
    {
        _context = context;
        _logger = logger;
        _settings = settings;
    }

    public string GenerateConnectionString(string databaseName)
    {
        return $"Server={_settings.DatabaseHost}; Database={databaseName}; Uid={_settings.DatabaseUserId}; Pwd={_settings.DatabasePassword}";
    }

    public async Task<bool> CreateDatabaseAsync(string connectionString)
    {
        try
        {
            await _context.Database.MigrateAsync();
            await AddTenantRoles();
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
            _logger.LogError("Thrown exception in MySqlProviderService.DeleteDatabaseAsync(): {@Exception}", ex);
            return false;
        }
    }

    private async Task AddTenantRoles()
    {
        foreach (var tenantRole in TenantRoles.GetValues())
        {
            var role = new TenantRole(tenantRole.Value)
            {
                DisplayName = tenantRole.DisplayName
            };

            var existingRole = await _context.Set<TenantRole>().FirstOrDefaultAsync(i => i.Name == role.Name);
            if (existingRole != null)
                continue;

            _context.Set<TenantRole>().Add(role);
        }

        await _context.SaveChangesAsync();
    }
}

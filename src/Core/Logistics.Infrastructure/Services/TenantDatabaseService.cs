using System.Data.Common;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Logistics.Domain.Entities;
using Logistics.Domain.Options;
using Logistics.Domain.Services;
using Logistics.Infrastructure.Data;
using Logistics.Shared.Consts.Policies;
using Logistics.Shared.Consts.Roles;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using CustomClaimTypes = Logistics.Shared.Consts.Claims.CustomClaimTypes;

namespace Logistics.Infrastructure.Services;

public class TenantDatabaseService : ITenantDatabaseService
{
    private readonly TenantDbContext _context;
    private readonly TenantsDatabaseOptions _databaseOptions;
    private readonly ILogger<TenantDatabaseService> _logger;

    public TenantDatabaseService(
        TenantDbContext context,
        TenantsDatabaseOptions databaseOptions,
        ILogger<TenantDatabaseService> logger)
    {
        _context = context;
        _logger = logger;
        _databaseOptions = databaseOptions;
    }

    public string GenerateConnectionString(string tenantName)
    {
        if (string.IsNullOrEmpty(_databaseOptions.DatabaseNameTemplate))
        {
            throw new InvalidOperationException(
                "The database name template is not defined in the TenantsDatabaseOptions appsettings.json file");
        }
        
        var databaseName = Regex.Replace(_databaseOptions.DatabaseNameTemplate, "{tenant}", tenantName);
        var connectionString =
            $"Host={_databaseOptions.DatabaseHost}; Database={databaseName}; Port=5432; Username={_databaseOptions.DatabaseUserId}; Password={_databaseOptions.DatabasePassword}";
        return connectionString;
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
            _logger.LogError("Thrown exception in TenantDatabaseService.CreateDatabaseAsync(): {Exception}", ex);
            return false;
        }
    }

    public async Task<bool> DeleteDatabaseAsync(string connectionString)
    {
        try
        {
            // Build a main connection for the database to drop
            var sourceBuilder = new NpgsqlConnectionStringBuilder(connectionString);
            var targetDbName = sourceBuilder.Database;

            // Switch to a different database like 'postgres' to perform the drop
            var masterBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = sourceBuilder.Host,
                Port = sourceBuilder.Port,
                Username = sourceBuilder.Username,
                Password = sourceBuilder.Password,
                Database = "postgres"
            };

            await using var connection = new NpgsqlConnection(masterBuilder.ConnectionString);
            await connection.OpenAsync();

            var dropQuery = $"DROP DATABASE {targetDbName}";
            await using var cmd = new NpgsqlCommand(dropQuery, connection);
            await cmd.ExecuteNonQueryAsync();
            return true;
        }
        catch (DbException ex)
        {
            _logger.LogError("Thrown exception in TenantDatabaseService.DeleteDatabaseAsync(): {@Exception}", ex);
            return false;
        }
    }

    private async Task AddTenantRoles()
    {
        var roles = TenantRoles.GetValues();

        foreach (var tenantRole in roles)
        {
            var role = new TenantRole(tenantRole.Value)
            {
                DisplayName = tenantRole.DisplayName
            };

            var existingRole = await _context.Set<TenantRole>().FirstOrDefaultAsync(i => i.Name == role.Name);
            if (existingRole != null)
            {
                // Update existing role claims
                var newPermissions = GetPermissionsBasedOnRole(role.Name);
                AddRolePermissions(existingRole, newPermissions);
                _context.Set<TenantRole>().Update(existingRole);
                _logger.LogInformation("Updated tenant role '{Role}'", existingRole.Name);
            }
            else
            {
                // Add new role and its claims
                AddRolePermissions(role, TenantRolePermissions.GetBasicPermissions());
                var newPermissions = GetPermissionsBasedOnRole(role.Name);
                AddRolePermissions(role, newPermissions);

                _context.Set<TenantRole>().Add(role);
                _logger.LogInformation("Added tenant role '{Role}'", role.Name);
            }
        }

        await _context.SaveChangesAsync();
    }

    private void AddRolePermissions(TenantRole role, IEnumerable<string> permissions)
    {
        foreach (var permission in permissions)
        {
            // Add the claim only if it does not already exist.
            if (!role.Claims.Any(c => c.ClaimType == CustomClaimTypes.Permission && c.ClaimValue == permission))
            {
                var claim = new Claim(CustomClaimTypes.Permission, permission);
                role.Claims.Add(new TenantRoleClaim(claim.Type, claim.Value));
                
                _logger.LogInformation("Added claim '{ClaimType}' - '{ClaimValue}' to the tenant role '{Role}'",
                    claim.Type, claim.Value, role.Name);
            }
        }
    }
    
    private static IEnumerable<string> GetPermissionsBasedOnRole(string roleName)
    {
        return roleName switch
        {
            TenantRoles.Owner => TenantRolePermissions.Owner,
            TenantRoles.Manager => TenantRolePermissions.Manager,
            TenantRoles.Dispatcher => TenantRolePermissions.Dispatcher,
            TenantRoles.Driver => TenantRolePermissions.Driver,
            _ => []
        };
    }
}

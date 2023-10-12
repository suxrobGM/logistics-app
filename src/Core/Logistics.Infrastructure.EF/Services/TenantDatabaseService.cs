using System.Data.Common;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Logistics.Domain.ValueObjects;
using Logistics.Shared.Policies;
using Microsoft.Extensions.Logging;
using Microsoft.Data.SqlClient;
using CustomClaimTypes = Logistics.Shared.Claims.CustomClaimTypes;

namespace Logistics.Infrastructure.EF.Services;

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
            $"Server={_databaseOptions.DatabaseHost}; Database={databaseName}; Uid={_databaseOptions.DatabaseUserId}; Pwd={_databaseOptions.DatabasePassword}; TrustServerCertificate=true";
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
            var connection = new DbConnectionStringBuilder
            {
                ConnectionString = connectionString
            };

            var database = connection["Initial Catalog"];
            var dropQuery = $"DROP DATABASE '{database}'";
            await using var sqlCommand = new SqlCommand(dropQuery);
            await sqlCommand.ExecuteScalarAsync();
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
            _ => Enumerable.Empty<string>()
        };
    }
}

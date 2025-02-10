using Logistics.Domain.Options;
using Logistics.Infrastructure.EF.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace Logistics.IdentityServer.Pages.Debug;

[AllowAnonymous]
public class Index : PageModel
{
    private readonly MasterDbContextOptions _masterDbContextOptions;
    private readonly TenantDbContextOptions _tenantDbContextOptions;
    private readonly TenantsDatabaseOptions _tenantsDatabaseOptions;
    
    public Index(
        MasterDbContextOptions masterDbContextOptions, 
        TenantDbContextOptions tenantDbContextOptions, 
        TenantsDatabaseOptions tenantsDatabaseOptions)
    {
        _masterDbContextOptions = masterDbContextOptions;
        _tenantDbContextOptions = tenantDbContextOptions;
        _tenantsDatabaseOptions = tenantsDatabaseOptions;
    }

    public ViewModel View { get; set; } = new();

    public void OnGet()
    {
        PopulateViewModel();
    }
    
    public async Task<IActionResult> OnPostTestMasterDbAsync()
    {
        PopulateViewModel();
        View.TestMasterDbResult = await TestConnection(_masterDbContextOptions.ConnectionString, "Master");
        return Page();
    }

    public async Task<IActionResult> OnPostTestTenantDbAsync()
    {
        PopulateViewModel();
        View.TestTenantDbResult = await TestConnection(_tenantDbContextOptions.ConnectionString, "Tenant");
        return Page();
    }
    
    private void PopulateViewModel()
    {
        View.MasterDbConnectionString = _masterDbContextOptions.ConnectionString;
        View.TenantDbConnectionString = _tenantDbContextOptions.ConnectionString;
        View.TenantsDatabaseSettings = _tenantsDatabaseOptions.ToString();
    }
    
    private async Task<string> TestConnection(string connectionString, string databaseName)
    {
        if (string.IsNullOrEmpty(connectionString))
            return $"No connection string provided for {databaseName}";

        try
        {
            await using var connection = new SqlConnection(connectionString);
            await connection.OpenAsync();
            return $"Successfully connected to {databaseName}";
        }
        catch (Exception ex)
        {
            return $"Failed to connect to {databaseName}: {ex.Message}";
        }
    }
}
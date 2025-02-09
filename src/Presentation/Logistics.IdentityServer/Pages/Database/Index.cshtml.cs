using Logistics.Domain.Options;
using Logistics.Infrastructure.EF.Options;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

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
        View.MasterDbConnectionString = _masterDbContextOptions.ConnectionString;
        View.TenantDbConnectionString = _tenantDbContextOptions.ConnectionString;
        View.TenantsDatabaseSettings = _tenantsDatabaseOptions.ToString();
    }
}
namespace Logistics.IdentityServer.Pages.Debug;

public class ViewModel
{
    public string MasterDbConnectionString { get; set; }
    public string TenantDbConnectionString { get; set; }
    public string TenantsDatabaseSettings { get; set; }
    public string TestMasterDbResult { get; set; }
    public string TestTenantDbResult { get; set; }
}
namespace Logistics.Application.Contracts.Services;

internal interface ITenantService
{
    public string GetDatabaseProvider();
    public string GetConnectionString();
    //public Tenant GetTenant();
}

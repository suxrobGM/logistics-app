namespace Logistics.WebApi.Client;

public interface IApiClient : ICargoApi, ITruckApi, IEmployeeApi, ITenantApi
{
    void SetCurrentTenantId(string? tenantId);
    string? CurrentTenantId { get; }
}

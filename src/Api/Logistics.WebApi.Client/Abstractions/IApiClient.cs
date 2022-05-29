namespace Logistics.WebApi.Client;

public interface IApiClient : ICargoApi, ITruckApi, IUserApi, ITenantApi
{
    void SetCurrentTenantId(string? tenantId);
    string? CurrentTenantId { get; }
}

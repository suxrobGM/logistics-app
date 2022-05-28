namespace Logistics.WebApi.Client;

public interface IApiClient : ICargoApi, ITruckApi, IUserApi, ITenantApi
{
    void SetTenantId(string? tenantId);
    string? TenantId { get; }
}

namespace Logistics.WebApi.Client;

public interface IApiClient : ILoadApi, ITruckApi, IEmployeeApi, ITenantApi
{
    string? AccessToken { get; set; }
}

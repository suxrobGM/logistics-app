namespace Logistics.WebApi.Client;

public interface IApiClient : ICargoApi, ITruckApi, IEmployeeApi, ITenantApi
{
    string? AccessToken { get; set; }
}

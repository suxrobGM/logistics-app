using Logistics.Client.Abstractions;

namespace Logistics.Client;

public interface IApiClient : 
    ILoadApi, 
    ITruckApi, 
    IEmployeeApi, 
    ITenantApi, 
    IUserApi, 
    IDriverApi,
    IStatsApi,
    ISubscriptionApi
{
    string? AccessToken { get; set; }
    string? TenantId { get; set; }
    public event EventHandler<string>? OnErrorResponse;
}

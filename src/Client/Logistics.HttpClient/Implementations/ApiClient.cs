using System.IdentityModel.Tokens.Jwt;

using Logistics.HttpClient.Exceptions;
using Logistics.HttpClient.Options;
using Logistics.Shared.Models;

namespace Logistics.HttpClient.Implementations;

internal class ApiClient : GenericApiClient, IApiClient
{
    private string? _accessToken;
    private Guid? _tenantId;

    public ApiClient(ApiClientOptions options) : base(options.Host!)
    {
        AccessToken = options.AccessToken;
    }

    public event EventHandler<string>? OnErrorResponse;

    public string? AccessToken
    {
        get => _accessToken;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }

            _accessToken = value;
            SetAuthorizationHeader("Bearer", _accessToken);
            SetTenantIdFromAccessToken(_accessToken);
        }
    }

    public Guid? TenantId
    {
        get => _tenantId;
        set
        {
            _tenantId = value;
            SetRequestHeader("X-Tenant", _tenantId?.ToString());
        }
    }

    private void SetTenantIdFromAccessToken(string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return;
        }

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(accessToken);
        var tenantId = token?.Claims?.FirstOrDefault(i => i.Type == "tenant")?.Value;


        if (!Guid.TryParse(tenantId, out var tenantGuid) || TenantId == tenantGuid)
        {
            return;
        }

        TenantId = tenantGuid;
        SetRequestHeader("X-Tenant", tenantId);
    }

    private async Task<TRes?> MakeGetRequestAsync<TRes>(string endpoint, IDictionary<string, string>? query = null)
        where TRes : class
    {
        try
        {
            return await GetRequestAsync<TRes>(endpoint, query);
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return null;
        }
    }

    private async Task<bool> MakePostRequestAsync<TBody>(string endpoint, TBody body)
        where TBody : class, new()
    {
        try
        {
            await PostRequestAsync(endpoint, body);
            return true;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return false;
        }
    }

    private async Task<bool> MakePutRequestAsync<TBody>(string endpoint, TBody body)
        where TBody : class, new()
    {
        try
        {
            await PutRequestAsync(endpoint, body);
            return true;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return false;
        }
    }

    private async Task<bool> MakeDeleteRequestAsync(string endpoint)
    {
        try
        {
            await DeleteRequestAsync(endpoint);
            return true;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return false;
        }
    }


    #region Load API

    public Task<LoadDto?> GetLoadAsync(Guid id)
    {
        return MakeGetRequestAsync<LoadDto>($"loads/{id}");
    }

    public Task<PagedResponse<LoadDto>?> GetLoadsAsync(GetLoadsQuery query)
    {
        return MakeGetRequestAsync<PagedResponse<LoadDto>>("loads", query.ToDictionary());
    }

    public async Task<ICollection<LoadDto>?> GetDriverActiveLoadsAsync(Guid userId)
    {
        var query = new Dictionary<string, string>
        {
            { "userId", userId.ToString() },
            { "onlyActiveLoads", "true" },
            { "loadAllPage", "true" }
        };
        var result = await MakeGetRequestAsync<PagedResponse<LoadDto>>("loads", query);
        return result?.Items.ToList();
    }

    public Task<bool> CreateLoadAsync(CreateLoadCommand command)
    {
        return MakePostRequestAsync("loads", command);
    }

    public Task<bool> UpdateLoadAsync(UpdateLoadCommand command)
    {
        return MakePutRequestAsync($"loads/{command.Id}", command);
    }

    public Task<bool> DeleteLoadAsync(Guid id)
    {
        return MakeDeleteRequestAsync($"loads/{id}");
    }

    #endregion


    #region Truck API

    public Task<TruckDto?> GetTruckAsync(GetTruckQuery query)
    {
        var id = query.TruckOrDriverId;
        return MakeGetRequestAsync<TruckDto>($"trucks/{id}", query.ToDictionary());
    }

    public Task<PagedResponse<TruckDto>?> GetTrucksAsync(SearchableQuery query, bool includeLoads = false)
    {
        var queryDict = query.ToDictionary();
        queryDict.Add("includeLoads", includeLoads.ToString());
        return MakeGetRequestAsync<PagedResponse<TruckDto>>("trucks", queryDict);
    }

    public Task<bool> CreateTruckAsync(CreateTruckCommand command)
    {
        return MakePostRequestAsync("trucks", command);
    }

    public Task<bool> UpdateTruckAsync(UpdateTruckCommand command)
    {
        return MakePutRequestAsync($"trucks/{command.Id}", command);
    }

    public Task<bool> DeleteTruckAsync(Guid id)
    {
        return MakeDeleteRequestAsync($"trucks/{id}");
    }

    #endregion


    #region Employee API

    public Task<EmployeeDto?> GetEmployeeAsync(Guid userId)
    {
        return MakeGetRequestAsync<EmployeeDto>($"employees/{userId}");
    }

    public Task<PagedResponse<EmployeeDto>?> GetEmployeesAsync(SearchableQuery query)
    {
        return MakeGetRequestAsync<PagedResponse<EmployeeDto>>("employees", query.ToDictionary());
    }

    public Task<bool> CreateEmployeeAsync(CreateEmployeeCommand command)
    {
        return MakePostRequestAsync("employees", command);
    }

    public Task<bool> UpdateEmployeeAsync(UpdateEmployeeCommand command)
    {
        return MakePutRequestAsync($"employees/{command.UserId}", command);
    }

    public Task<bool> DeleteEmployeeAsync(Guid userId)
    {
        return MakeDeleteRequestAsync($"employees/{userId}");
    }

    #endregion


    #region Tenant API

    public Task<TenantDto?> GetTenantAsync(string identifier)
    {
        return MakeGetRequestAsync<TenantDto>($"tenants/{identifier}");
    }

    public Task<PagedResponse<TenantDto>?> GetTenantsAsync(SearchableQuery query)
    {
        return MakeGetRequestAsync<PagedResponse<TenantDto>>("tenants", query.ToDictionary());
    }

    public Task<bool> CreateTenantAsync(CreateTenantCommand command)
    {
        return MakePostRequestAsync("tenants", command);
    }

    public Task<bool> UpdateTenantAsync(UpdateTenantCommand command)
    {
        return MakePutRequestAsync($"tenants/{command.Id}", command);
    }

    public Task<bool> DeleteTenantAsync(Guid id)
    {
        return MakeDeleteRequestAsync($"tenants/{id}");
    }

    #endregion


    #region User API

    public Task<UserDto?> GetUserAsync(Guid userId)
    {
        return MakeGetRequestAsync<UserDto>($"users/{userId}");
    }

    public Task<PagedResponse<UserDto>?> GetUsersAsync(SearchableQuery query)
    {
        return MakeGetRequestAsync<PagedResponse<UserDto>>("users", query.ToDictionary());
    }

    public Task<bool> UpdateUserAsync(UpdateUser command)
    {
        return MakePutRequestAsync($"users/{command.UserId}", command);
    }

    public Task<TenantDto?> GetUserCurrentTenant(Guid userId)
    {
        return MakeGetRequestAsync<TenantDto>($"users/{userId}/tenant");
    }

    public Task<string[]?> GetCurrentUserPermissionsAsync()
    {
        return MakeGetRequestAsync<string[]>("users/me/permissions");
    }

    #endregion


    #region Driver API

    public Task<bool> SetDeviceTokenAsync(SetDeviceToken command)
    {
        return MakePostRequestAsync($"drivers/{command.UserId}/device-token", command);
    }

    public Task<bool> ConfirmLoadStatusAsync(ConfirmLoadStatus command)
    {
        return MakePostRequestAsync("drivers/confirm-load-status", command);
    }

    public Task<bool> UpdateLoadProximity(UpdateLoadProximityCommand command)
    {
        return MakePostRequestAsync("drivers/update-load-proximity", command);
    }

    #endregion


    #region Stats API

    public Task<DailyGrossesDto?> GetDailyGrossesAsync(GetDailyGrossesQuery query)
    {
        return MakeGetRequestAsync<DailyGrossesDto>("stats/daily-grosses", query.ToDictionary());
    }

    public Task<MonthlyGrossesDto?> GetMonthlyGrossesAsync(GetMonthlyGrossesQuery query)
    {
        return MakeGetRequestAsync<MonthlyGrossesDto>("stats/monthly-grosses", query.ToDictionary());
    }

    public Task<DriverStatsDto?> GetDriverStatsAsync(Guid userId)
    {
        return MakeGetRequestAsync<DriverStatsDto>($"stats/driver/{userId}");
    }

    #endregion


    #region Subscriptions API

    public Task<SubscriptionDto?> GetSubscriptionAsync(Guid id)
    {
        return MakeGetRequestAsync<SubscriptionDto>($"subscriptions/{id}");
    }

    public Task<SubscriptionPlanDto?> GetSubscriptionPlanAsync(Guid planId)
    {
        return MakeGetRequestAsync<SubscriptionPlanDto>($"subscriptions/plans/{planId}");
    }

    public Task<PagedResponse<SubscriptionDto>?> GetSubscriptionsAsync(PagedQuery query)
    {
        return MakeGetRequestAsync<PagedResponse<SubscriptionDto>>("subscriptions", query.ToDictionary());
    }

    public Task<PagedResponse<SubscriptionPlanDto>?> GetSubscriptionPlansAsync(PagedQuery query)
    {
        return MakeGetRequestAsync<PagedResponse<SubscriptionPlanDto>>("subscriptions/plans", query.ToDictionary());
    }

    public Task<bool> CreateSubscriptionPlanAsync(CreateSubscriptionPlanCommand command)
    {
        return MakePostRequestAsync("subscriptions/plans", command);
    }

    public Task<bool> UpdateSubscriptionPlanAsync(UpdateSubscriptionPlanCommand command)
    {
        return MakePutRequestAsync($"subscriptions/plans/{command.Id}", command);
    }

    public Task<bool> DeleteSubscriptionPlanAsync(Guid id)
    {
        return MakeDeleteRequestAsync($"subscriptions/plans/{id}");
    }

    public Task<bool> CreateSubscriptionAsync(CreateSubscriptionCommand command)
    {
        return MakePostRequestAsync("subscriptions", command);
    }

    public Task<bool> DeleteSubscriptionAsync(Guid id)
    {
        return MakeDeleteRequestAsync($"subscriptions/{id}");
    }

    public Task<bool> CancelSubscriptionAsync(CancelSubscriptionCommand command)
    {
        return MakePutRequestAsync($"subscriptions/{command.Id}/cancel", command);
    }

    #endregion
}

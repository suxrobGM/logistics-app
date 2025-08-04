using System.IdentityModel.Tokens.Jwt;
using Logistics.Shared.Models;
using Logistics.HttpClient.Exceptions;
using Logistics.HttpClient.Options;

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

    private async Task<TRes> MakeGetRequestAsync<TRes>(string endpoint, IDictionary<string, string>? query = null)
        where TRes : class, IResult, new()
    {
        try
        {
            var result = await GetRequestAsync<TRes>(endpoint, query);

            if (!result.Success)
            {
                OnErrorResponse?.Invoke(this, result.Error!);
            }

            return result;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return new TRes { Error = ex.Message };
        }
    }

    private async Task<TRes> MakePostRequestAsync<TRes, TBody>(string endpoint, TBody body)
        where TRes : class, IResult, new()
        where TBody : class, new()
    {
        try
        {
            var result = await PostRequestAsync<TRes, TBody>(endpoint, body);

            if (!result.Success)
            {
                OnErrorResponse?.Invoke(this, result.Error!);
            }

            return result;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return new TRes { Error = ex.Message };
        }
    }

    private async Task<TRes> MakePutRequestAsync<TRes, TBody>(string endpoint, TBody body)
        where TRes : class, IResult, new()
        where TBody : class, new()
    {
        try
        {
            var result = await PutRequestAsync<TRes, TBody>(endpoint, body);

            if (!result.Success)
            {
                OnErrorResponse?.Invoke(this, result.Error!);
            }

            return result;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return new TRes { Error = ex.Message };
        }
    }

    private async Task<TRes> MakeDeleteRequestAsync<TRes>(string endpoint)
        where TRes : class, IResult, new()
    {
        try
        {
            var result = await DeleteRequestAsync<TRes>(endpoint);

            if (!result.Success)
            {
                OnErrorResponse?.Invoke(this, result.Error!);
            }

            return result;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return new TRes { Error = ex.Message };
        }
    }


    #region Load API
    
    public Task<Result<LoadDto>> GetLoadAsync(Guid id)
    {
        return MakeGetRequestAsync<Result<LoadDto>>($"loads/{id}");
    }

    public Task<PagedResult<LoadDto>> GetLoadsAsync(GetLoadsQuery query)
    {
        return MakeGetRequestAsync<PagedResult<LoadDto>>("loads", query.ToDictionary());
    }
    
    public Task<Result<ICollection<LoadDto>>> GetDriverActiveLoadsAsync(Guid userId)
    {
        var query = new Dictionary<string, string>
        {
            { "userId", userId.ToString() },
            { "onlyActiveLoads", "true" },
            { "loadAllPage", "true" }
        };
        return MakeGetRequestAsync<Result<ICollection<LoadDto>>>("loads", query);
    }

    public Task<Result> CreateLoadAsync(CreateLoad command)
    {
        return MakePostRequestAsync<Result, CreateLoad>("loads", command);
    }
    
    public Task<Result> UpdateLoadAsync(UpdateLoad command)
    {
        return MakePutRequestAsync<Result, UpdateLoad>($"loads/{command.Id}", command);
    }

    public Task<Result> DeleteLoadAsync(Guid id)
    {
        return MakeDeleteRequestAsync<Result>($"loads/{id}");
    }

    #endregion


    #region Truck API

    public Task<Result<TruckDto>> GetTruckAsync(GetTruckQuery query)
    {
        var id = query.TruckOrDriverId;
        return MakeGetRequestAsync<Result<TruckDto>>($"trucks/{id}", query.ToDictionary());
    }

    public Task<PagedResult<TruckDto>> GetTrucksAsync(SearchableQuery query, bool includeLoads = false)
    {
        var queryDict = query.ToDictionary();
        queryDict.Add("includeLoads", includeLoads.ToString());
        return MakeGetRequestAsync<PagedResult<TruckDto>>("trucks", queryDict);
    }

    public Task<Result> CreateTruckAsync(CreateTruckCommand command)
    {
        return MakePostRequestAsync<Result, CreateTruckCommand>("trucks", command);
    }

    public Task<Result> UpdateTruckAsync(UpdateTruckCommand command)
    {
        return MakePutRequestAsync<Result, UpdateTruckCommand>($"trucks/{command.Id}", command);
    }

    public Task<Result> DeleteTruckAsync(Guid id)
    {
        return MakeDeleteRequestAsync<Result>($"trucks/{id}");
    }

    #endregion


    #region Employee API

    public Task<Result<EmployeeDto>> GetEmployeeAsync(Guid userId)
    {
        return MakeGetRequestAsync<Result<EmployeeDto>>($"employees/{userId}");
    }

    public Task<PagedResult<EmployeeDto>> GetEmployeesAsync(SearchableQuery query)
    {
        return MakeGetRequestAsync<PagedResult<EmployeeDto>>("employees", query.ToDictionary());
    }

    public Task<Result> CreateEmployeeAsync(CreateEmployeeCommand command)
    {
        return MakePostRequestAsync<Result, CreateEmployeeCommand>("employees", command);
    }

    public Task<Result> UpdateEmployeeAsync(UpdateEmployeeCommand command)
    {
        return MakePutRequestAsync<Result, UpdateEmployeeCommand>($"employees/{command.UserId}", command);
    }
    
    public Task<Result> DeleteEmployeeAsync(Guid userId)
    {
        return MakeDeleteRequestAsync<Result>($"employees/{userId}");
    }

    #endregion


    #region Tenant API

    public Task<Result<TenantDto>> GetTenantAsync(string identifier)
    {
        return MakeGetRequestAsync<Result<TenantDto>>($"tenants/{identifier}");
    }

    public Task<PagedResult<TenantDto>> GetTenantsAsync(SearchableQuery query)
    {
        return MakeGetRequestAsync<PagedResult<TenantDto>>("tenants", query.ToDictionary());
    }

    public Task<Result> CreateTenantAsync(CreateTenantCommand command)
    {
        return MakePostRequestAsync<Result, CreateTenantCommand>("tenants", command);
    }

    public Task<Result> UpdateTenantAsync(UpdateTenantCommand command)
    {
        return MakePutRequestAsync<Result, UpdateTenantCommand>($"tenants/{command.Id}", command);
    }

    public Task<Result> DeleteTenantAsync(Guid id)
    {
        return MakeDeleteRequestAsync<Result>($"tenants/{id}");
    }

    #endregion


    #region User API

    public Task<Result<UserDto>> GetUserAsync(Guid userId)
    {
        return MakeGetRequestAsync<Result<UserDto>>($"users/{userId}");
    }

    public Task<PagedResult<UserDto>> GetUsersAsync(SearchableQuery query)
    {
        return MakeGetRequestAsync<PagedResult<UserDto>>("users", query.ToDictionary());
    }

    public Task<Result> UpdateUserAsync(UpdateUser command)
    {
        return MakePutRequestAsync<Result, UpdateUser>($"users/{command.UserId}", command);
    }

    public Task<Result<TenantDto>> GetUserCurrentTenant(Guid userId)
    {
        return MakeGetRequestAsync<Result<TenantDto>>($"users/{userId}/tenant");
    }

    #endregion


    #region Driver API
    
    public Task<Result> SetDeviceTokenAsync(SetDeviceToken command)
    {
        return MakePostRequestAsync<Result, SetDeviceToken>($"drivers/{command.UserId}/device-token", command);
    }

    public Task<Result> ConfirmLoadStatusAsync(ConfirmLoadStatus command)
    {
        return MakePostRequestAsync<Result, ConfirmLoadStatus>("drivers/confirm-load-status", command);
    }

    public Task<Result> UpdateLoadProximity(UpdateLoadProximity command)
    {
        return MakePostRequestAsync<Result, UpdateLoadProximity>("drivers/update-load-proximity", command);
    }

    #endregion


    #region Stats API

    public Task<Result<DailyGrossesDto>> GetDailyGrossesAsync(GetDailyGrossesQuery query)
    {
        return MakeGetRequestAsync<Result<DailyGrossesDto>>("stats/daily-grosses", query.ToDictionary());
    }

    public Task<Result<MonthlyGrossesDto>> GetMonthlyGrossesAsync(GetMonthlyGrossesQuery query)
    {
        return MakeGetRequestAsync<Result<MonthlyGrossesDto>>("stats/monthly-grosses", query.ToDictionary());
    }

    public Task<Result<DriverStatsDto>> GetDriverStatsAsync(Guid userId)
    {
        return MakeGetRequestAsync<Result<DriverStatsDto>>($"stats/driver/{userId}");  
    }

    #endregion

    
    #region Subscriptions API

    public Task<Result<SubscriptionDto>> GetSubscriptionAsync(Guid id)
    {
        return MakeGetRequestAsync<Result<SubscriptionDto>>($"subscriptions/{id}");
    }

    public Task<Result<SubscriptionPlanDto>> GetSubscriptionPlanAsync(Guid planId)
    {
        return MakeGetRequestAsync<Result<SubscriptionPlanDto>>($"subscriptions/plans/{planId}");
    }

    public Task<PagedResult<SubscriptionDto>> GetSubscriptionsAsync(PagedQuery query)
    {
        return MakeGetRequestAsync<PagedResult<SubscriptionDto>>("subscriptions", query.ToDictionary());
    }

    public Task<PagedResult<SubscriptionPlanDto>> GetSubscriptionPlansAsync(PagedQuery query)
    {
        return MakeGetRequestAsync<PagedResult<SubscriptionPlanDto>>("subscriptions/plans", query.ToDictionary());
    }

    public Task<Result> CreateSubscriptionPlanAsync(CreateSubscriptionPlanCommand command)
    {
        return MakePostRequestAsync<Result, CreateSubscriptionPlanCommand>("subscriptions/plans", command);
    }

    public Task<Result> UpdateSubscriptionPlanAsync(UpdateSubscriptionPlanCommand command)
    {
        return MakePutRequestAsync<Result, UpdateSubscriptionPlanCommand>($"subscriptions/plans/{command.Id}", command);
    }

    public Task<Result> DeleteSubscriptionPlanAsync(Guid id)
    {
        return MakeDeleteRequestAsync<Result>($"subscriptions/plans/{id}");
    }

    public Task<Result> CreateSubscriptionAsync(CreateSubscriptionCommand command)
    {
        return MakePostRequestAsync<Result, CreateSubscriptionCommand>("subscriptions", command);
    }

    public Task<Result> DeleteSubscriptionAsync(Guid id)
    {
        return MakeDeleteRequestAsync<Result>($"subscriptions/{id}");
    }
    
    public Task<Result> CancelSubscriptionAsync(CancelSubscriptionCommand command)
    {
        return MakePutRequestAsync<Result, CancelSubscriptionCommand>($"subscriptions/{command.Id}/cancel", command);
    }

    #endregion
}

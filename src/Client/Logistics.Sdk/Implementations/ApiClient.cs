using System.IdentityModel.Tokens.Jwt;
using Logistics.Sdk.Options;

namespace Logistics.Sdk.Implementations;

internal class ApiClient : GenericApiClient, IApiClient
{
    private string? _accessToken;
    private string? _tenantId;
    
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
                return;

            _accessToken = value;
            SetAuthorizationHeader("Bearer", _accessToken);
            SetTenantIdFromAccessToken(_accessToken);
        }
    }

    public string? TenantId 
    {
        get => _tenantId;
        set
        {
            _tenantId = value;
            SetRequestHeader("X-Tenant", _tenantId);
        }
    }
    
    private void SetTenantIdFromAccessToken(string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
            return;
        
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(accessToken);
        var tenantId = token?.Claims?.FirstOrDefault(i => i.Type == "tenant")?.Value;
        
        if (string.IsNullOrEmpty(tenantId) || TenantId == tenantId)
        {
            return;
        } 

        TenantId = tenantId;
        SetRequestHeader("X-Tenant", tenantId);
    }

    private async Task<TRes> MakeGetRequestAsync<TRes>(string endpoint, IDictionary<string, string>? query = null)
        where TRes : class, IResponseResult, new()
    {
        try
        {
            var result = await GetRequestAsync<TRes>(endpoint, query);

            if (!result.Success)
                OnErrorResponse?.Invoke(this, result.Error!);

            return result;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return new TRes() { Error = ex.Message };
        }
    }

    private async Task<TRes> MakePostRequestAsync<TRes, TBody>(string endpoint, TBody body)
        where TRes : class, IResponseResult, new()
        where TBody : class, new()
    {
        try
        {
            var result = await PostRequestAsync<TRes, TBody>(endpoint, body);

            if (!result.Success)
                OnErrorResponse?.Invoke(this, result.Error!);

            return result;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return new TRes() { Error = ex.Message };
        }
    }

    private async Task<TRes> MakePutRequestAsync<TRes, TBody>(string endpoint, TBody body)
        where TRes : class, IResponseResult, new()
        where TBody : class, new()
    {
        try
        {
            var result = await PutRequestAsync<TRes, TBody>(endpoint, body);

            if (!result.Success)
                OnErrorResponse?.Invoke(this, result.Error!);

            return result;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return new TRes() { Error = ex.Message };
        }
    }

    private async Task<TRes> MakeDeleteRequestAsync<TRes>(string endpoint)
        where TRes : class, IResponseResult, new()
    {
        try
        {
            var result = await DeleteRequestAsync<TRes>(endpoint);

            if (!result.Success)
                OnErrorResponse?.Invoke(this, result.Error!);

            return result;
        }
        catch (ApiException ex)
        {
            OnErrorResponse?.Invoke(this, ex.Message);
            return new TRes() { Error = ex.Message };
        }
    }


    #region Load API

    public Task<ResponseResult<Load>> GetLoadAsync(string id)
    {
        return MakeGetRequestAsync<ResponseResult<Load>>($"load/{id}");
    }

    public Task<PagedResponseResult<Load>> GetLoadsAsync(SearchableQuery query)
    {
        return MakeGetRequestAsync<PagedResponseResult<Load>>("load/list", query.ToDictionary());
    }

    public Task<ResponseResult> CreateLoadAsync(CreateLoad load)
    {
        return MakePostRequestAsync<ResponseResult, CreateLoad>("load/create", load);
    }
    
    public Task<ResponseResult> UpdateLoadAsync(UpdateLoad load)
    {
        return MakePutRequestAsync<ResponseResult, UpdateLoad>($"load/update/{load.Id}", load);
    }

    public Task<ResponseResult> DeleteLoadAsync(string id)
    {
        return MakeDeleteRequestAsync<ResponseResult>($"load/delete/{id}");
    }

    #endregion


    #region Truck API

    public Task<ResponseResult<Truck>> GetTruckAsync(string id)
    {
        return MakeGetRequestAsync<ResponseResult<Truck>>($"truck/{id}");
    }

    public Task<ResponseResult<Truck>> GetTruckByDriverAsync(string driverId)
    {
        return MakeGetRequestAsync<ResponseResult<Truck>>($"truck/getByDriver?driverId={driverId}");  
    }

    public Task<PagedResponseResult<Truck>> GetTrucksAsync(SearchableQuery query, bool includeCargoIds = false)
    {
        var queryDict = query.ToDictionary();
        queryDict.Add("includeCargoIds", includeCargoIds.ToString());
        return MakeGetRequestAsync<PagedResponseResult<Truck>>("truck/list", queryDict);
    }

    public Task<ResponseResult> CreateTruckAsync(CreateTruck truck)
    {
        return MakePostRequestAsync<ResponseResult, CreateTruck>("truck/create", truck);
    }

    public Task<ResponseResult> UpdateTruckAsync(UpdateTruck truck)
    {
        return MakePutRequestAsync<ResponseResult, UpdateTruck>($"truck/update/{truck.Id}", truck);
    }

    public Task<ResponseResult> DeleteTruckAsync(string id)
    {
        return MakeDeleteRequestAsync<ResponseResult>($"truck/delete/{id}");
    }

    #endregion


    #region Employee API

    public Task<ResponseResult<Employee>> GetEmployeeAsync(string id)
    {
        return MakeGetRequestAsync<ResponseResult<Employee>>($"employee/{id}");
    }

    public Task<PagedResponseResult<Employee>> GetEmployeesAsync(SearchableQuery query)
    {
        return MakeGetRequestAsync<PagedResponseResult<Employee>>("employee/list", query.ToDictionary());
    }

    public Task<ResponseResult> CreateEmployeeAsync(CreateEmployee employee)
    {
        return MakePostRequestAsync<ResponseResult, CreateEmployee>("employee/create", employee);
    }

    public Task<ResponseResult> UpdateEmployeeAsync(UpdateEmployee employee)
    {
        return MakePutRequestAsync<ResponseResult, UpdateEmployee>($"employee/update/{employee.UserId}", employee);
    }
    
    public Task<ResponseResult> DeleteEmployeeAsync(string id)
    {
        return MakeDeleteRequestAsync<ResponseResult>($"employee/delete/{id}");
    }

    #endregion


    #region Tenant API

    public Task<ResponseResult<string>> GetTenantDisplayNameAsync(string id)
    {
        return MakeGetRequestAsync<ResponseResult<string>>($"tenant/getDisplayName?id={id}");
    }

    public Task<ResponseResult<Tenant>> GetTenantAsync(string identifier)
    {
        return MakeGetRequestAsync<ResponseResult<Tenant>>($"tenant/{identifier}");
    }

    public Task<PagedResponseResult<Tenant>> GetTenantsAsync(SearchableQuery query)
    {
        return MakeGetRequestAsync<PagedResponseResult<Tenant>>("tenant/list", query.ToDictionary());
    }

    public Task<ResponseResult> CreateTenantAsync(CreateTenant tenant)
    {
        return MakePostRequestAsync<ResponseResult, CreateTenant>("tenant/create", tenant);
    }

    public Task<ResponseResult> UpdateTenantAsync(UpdateTenant tenant)
    {
        return MakePutRequestAsync<ResponseResult, UpdateTenant>($"tenant/update/{tenant.Id}", tenant);
    }

    public Task<ResponseResult> DeleteTenantAsync(string id)
    {
        return MakeDeleteRequestAsync<ResponseResult>($"tenant/delete/{id}");
    }

    #endregion


    #region User API

    public Task<ResponseResult<User>> GetUserAsync(string id)
    {
        return MakeGetRequestAsync<ResponseResult<User>>($"user/{id}");
    }

    public Task<ResponseResult> UpdateUserAsync(UpdateUser user)
    {
        return MakePutRequestAsync<ResponseResult, UpdateUser>($"user/update/{user.Id}", user);
    }

    #endregion
}

using System.IdentityModel.Tokens.Jwt;
using Logistics.Sdk.Options;

namespace Logistics.Sdk.Implementations;

internal class ApiClient : GenericApiClient, IApiClient
{
    private string? _accessToken;
    private string? _currentTenant;
    
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
            SetCurrentTenantId(_accessToken);
        }
    }
    
    private void SetCurrentTenantId(string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
            return;
        
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(accessToken);
        var tenantId = token?.Claims?.FirstOrDefault(i => i.Type == "tenant")?.Value;
        
        if (_currentTenant == tenantId)
            return;
        
        _currentTenant = tenantId;
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

    private async Task<TRes> MakePostRequestAsync<TRes, TData>(string endpoint, TData data)
        where TRes : class, IResponseResult, new()
        where TData : class, new()
    {
        try
        {
            var result = await PostRequestAsync<TRes, TData>(endpoint, data);

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

    private async Task<TRes> MakePutRequestAsync<TRes, TData>(string endpoint, TData data)
        where TRes : class, IResponseResult, new()
        where TData : class, new()
    {
        try
        {
            var result = await PutRequestAsync<TRes, TData>(endpoint, data);

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

    public Task<ResponseResult<LoadDto>> GetLoadAsync(string id)
    {
        return MakeGetRequestAsync<ResponseResult<LoadDto>>($"load/{id}");
    }

    public Task<PagedResponseResult<LoadDto>> GetLoadsAsync(string searchInput = "", int page = 1, int pageSize = 10)
    {
        var query = new Dictionary<string, string>
        {
            {"page", page.ToString() },
            {"pageSize", pageSize.ToString() }
        };

        if (!string.IsNullOrEmpty(searchInput))
        {
            query.Add("search", searchInput);
        }

        return MakeGetRequestAsync<PagedResponseResult<LoadDto>>("load/list", query);
    }

    public Task<ResponseResult> CreateLoadAsync(LoadDto load)
    {
        return MakePostRequestAsync<ResponseResult, LoadDto>("load/create", load);
    }
    
    public Task<ResponseResult> UpdateLoadAsync(LoadDto load)
    {
        return MakePutRequestAsync<ResponseResult, LoadDto>($"load/update/{load.Id}", load);
    }

    public Task<ResponseResult> DeleteLoadAsync(string id)
    {
        return MakeDeleteRequestAsync<ResponseResult>($"load/delete/{id}");
    }

    #endregion


    #region Truck API

    public Task<ResponseResult<TruckDto>> GetTruckAsync(string id)
    {
        return MakeGetRequestAsync<ResponseResult<TruckDto>>($"truck/{id}");
    }

    public Task<ResponseResult<TruckDto>> GetTruckByDriverAsync(string driverId)
    {
        return MakeGetRequestAsync<ResponseResult<TruckDto>>($"truck/driver/{driverId}");  
    }

    public Task<PagedResponseResult<TruckDto>> GetTrucksAsync(string searchInput = "", int page = 1, int pageSize = 10, bool includeCargoIds = false)
    {
        var query = new Dictionary<string, string>
        {
            {"page", page.ToString() },
            {"pageSize", pageSize.ToString() },
            {"includeCargoIds", includeCargoIds.ToString() }
        };

        if (!string.IsNullOrEmpty(searchInput))
        {
            query.Add("search", searchInput);
        }

        return MakeGetRequestAsync<PagedResponseResult<TruckDto>>("truck/list", query);
    }

    public Task<ResponseResult> CreateTruckAsync(TruckDto truck)
    {
        return MakePostRequestAsync<ResponseResult, TruckDto>("truck/create", truck);
    }

    public Task<ResponseResult> UpdateTruckAsync(TruckDto truck)
    {
        return MakePutRequestAsync<ResponseResult, TruckDto>($"truck/update/{truck.Id}", truck);
    }

    public Task<ResponseResult> DeleteTruckAsync(string id)
    {
        return MakeDeleteRequestAsync<ResponseResult>($"truck/delete/{id}");
    }

    #endregion


    #region Employee API

    public Task<ResponseResult<EmployeeDto>> GetEmployeeAsync(string id)
    {
        return MakeGetRequestAsync<ResponseResult<EmployeeDto>>($"employee/{id}");
    }

    public Task<PagedResponseResult<EmployeeDto>> GetEmployeesAsync(string searchInput = "", int page = 1, int pageSize = 10)
    {
        var query = new Dictionary<string, string>
        {
            {"page", page.ToString() },
            {"pageSize", pageSize.ToString() }
        };

        if (!string.IsNullOrEmpty(searchInput))
        {
            query.Add("search", searchInput);
        }

        return MakeGetRequestAsync<PagedResponseResult<EmployeeDto>>("employee/list", query);
    }

    public Task<ResponseResult> CreateEmployeeAsync(EmployeeDto user)
    {
        return MakePostRequestAsync<ResponseResult, EmployeeDto>("employee/create", user);
    }

    public Task<ResponseResult> UpdateEmployeeAsync(EmployeeDto user)
    {
        return MakePutRequestAsync<ResponseResult, EmployeeDto>($"employee/update/{user.Id}", user);
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

    public Task<ResponseResult<TenantDto>> GetTenantAsync(string identifier)
    {
        return MakeGetRequestAsync<ResponseResult<TenantDto>>($"tenant/{identifier}");
    }

    public Task<PagedResponseResult<TenantDto>> GetTenantsAsync(string searchInput = "", int page = 1, int pageSize = 10)
    {
        var query = new Dictionary<string, string>
        {
            {"page", page.ToString() },
            {"pageSize", pageSize.ToString() }
        };

        if (!string.IsNullOrEmpty(searchInput))
        {
            query.Add("search", searchInput);
        }

        return MakeGetRequestAsync<PagedResponseResult<TenantDto>>("tenant/list", query);
    }

    public Task<ResponseResult> CreateTenantAsync(TenantDto tenant)
    {
        return MakePostRequestAsync<ResponseResult, TenantDto>("tenant/create", tenant);
    }

    public Task<ResponseResult> UpdateTenantAsync(TenantDto tenant)
    {
        return MakePutRequestAsync<ResponseResult, TenantDto>($"tenant/update/{tenant.Id}", tenant);
    }

    public Task<ResponseResult> DeleteTenantAsync(string id)
    {
        return MakeDeleteRequestAsync<ResponseResult>($"tenant/delete/{id}");
    }

    #endregion
}

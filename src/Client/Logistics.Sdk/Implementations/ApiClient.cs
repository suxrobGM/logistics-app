using System.IdentityModel.Tokens.Jwt;
using Logistics.Application.Shared.Abstractions;
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
        where TRes : class, IDataResult, new()
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
        where TRes : class, IDataResult, new()
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
        where TRes : class, IDataResult, new()
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
        where TRes : class, IDataResult, new()
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

    public Task<DataResult<LoadDto>> GetLoadAsync(string id)
    {
        return MakeGetRequestAsync<DataResult<LoadDto>>($"load/{id}");
    }

    public Task<PagedDataResult<LoadDto>> GetLoadsAsync(string searchInput = "", int page = 1, int pageSize = 10)
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

        return MakeGetRequestAsync<PagedDataResult<LoadDto>>("load/list", query);
    }

    public Task<DataResult> CreateLoadAsync(LoadDto load)
    {
        return MakePostRequestAsync<DataResult, LoadDto>("load/create", load);
    }
    
    public Task<DataResult> UpdateLoadAsync(LoadDto load)
    {
        return MakePutRequestAsync<DataResult, LoadDto>($"load/update/{load.Id}", load);
    }

    public Task<DataResult> DeleteLoadAsync(string id)
    {
        return MakeDeleteRequestAsync<DataResult>($"load/delete/{id}");
    }

    #endregion


    #region Truck API

    public Task<DataResult<TruckDto>> GetTruckAsync(string id)
    {
        return MakeGetRequestAsync<DataResult<TruckDto>>($"truck/{id}");
    }

    public Task<DataResult<TruckDto>> GetTruckByDriverAsync(string driverId)
    {
        return MakeGetRequestAsync<DataResult<TruckDto>>($"truck/driver/{driverId}");  
    }

    public Task<PagedDataResult<TruckDto>> GetTrucksAsync(string searchInput = "", int page = 1, int pageSize = 10, bool includeCargoIds = false)
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

        return MakeGetRequestAsync<PagedDataResult<TruckDto>>("truck/list", query);
    }

    public Task<DataResult> CreateTruckAsync(TruckDto truck)
    {
        return MakePostRequestAsync<DataResult, TruckDto>("truck/create", truck);
    }

    public Task<DataResult> UpdateTruckAsync(TruckDto truck)
    {
        return MakePutRequestAsync<DataResult, TruckDto>($"truck/update/{truck.Id}", truck);
    }

    public Task<DataResult> DeleteTruckAsync(string id)
    {
        return MakeDeleteRequestAsync<DataResult>($"truck/delete/{id}");
    }

    #endregion


    #region Employee API

    public Task<DataResult<EmployeeDto>> GetEmployeeAsync(string id)
    {
        return MakeGetRequestAsync<DataResult<EmployeeDto>>($"employee/{id}");
    }

    public Task<PagedDataResult<EmployeeDto>> GetEmployeesAsync(string searchInput = "", int page = 1, int pageSize = 10)
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

        return MakeGetRequestAsync<PagedDataResult<EmployeeDto>>("employee/list", query);
    }

    public Task<DataResult> CreateEmployeeAsync(EmployeeDto user)
    {
        return MakePostRequestAsync<DataResult, EmployeeDto>("employee/create", user);
    }

    public Task<DataResult> UpdateEmployeeAsync(EmployeeDto user)
    {
        return MakePutRequestAsync<DataResult, EmployeeDto>($"employee/update/{user.Id}", user);
    }
    
    public Task<DataResult> DeleteEmployeeAsync(string id)
    {
        return MakeDeleteRequestAsync<DataResult>($"employee/delete/{id}");
    }

    #endregion


    #region Tenant API

    public Task<DataResult<string>> GetTenantDisplayNameAsync(string id)
    {
        return MakeGetRequestAsync<DataResult<string>>($"tenant/getDisplayName?id={id}");
    }

    public Task<DataResult<TenantDto>> GetTenantAsync(string identifier)
    {
        return MakeGetRequestAsync<DataResult<TenantDto>>($"tenant/{identifier}");
    }

    public Task<PagedDataResult<TenantDto>> GetTenantsAsync(string searchInput = "", int page = 1, int pageSize = 10)
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

        return MakeGetRequestAsync<PagedDataResult<TenantDto>>("tenant/list", query);
    }

    public Task<DataResult> CreateTenantAsync(TenantDto tenant)
    {
        return MakePostRequestAsync<DataResult, TenantDto>("tenant/create", tenant);
    }

    public Task<DataResult> UpdateTenantAsync(TenantDto tenant)
    {
        return MakePutRequestAsync<DataResult, TenantDto>($"tenant/update/{tenant.Id}", tenant);
    }

    public Task<DataResult> DeleteTenantAsync(string id)
    {
        return MakeDeleteRequestAsync<DataResult>($"tenant/delete/{id}");
    }

    #endregion
}

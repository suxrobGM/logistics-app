using System.IdentityModel.Tokens.Jwt;

namespace Logistics.WebApi.Client.Implementations;

internal class ApiClient : ApiClientBase, IApiClient
{
    private string? _accessToken;
    private string? _currentTenant;
    
    public ApiClient(ApiClientOptions options) : base(options.Host!)
    {
        AccessToken = options.AccessToken;
    }

    public string? AccessToken
    {
        get => _accessToken;
        set
        {
            if (string.IsNullOrEmpty(value))
            {
                return;
            }
            SetAuthorizationHeader("Bearer", value);
            SetCurrentTenantId(value);
            _accessToken = value;
        }
    }
    
    private void SetCurrentTenantId(string? accessToken)
    {
        if (string.IsNullOrEmpty(accessToken))
        {
            return;
        }
        
        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(accessToken);
        var tenantId = token?.Claims?.FirstOrDefault(i => i.Type == "tenant")?.Value;
        
        if (_currentTenant == tenantId)
            return;
        
        _currentTenant = tenantId;
        SetRequestHeader("X-Tenant", tenantId);
    }

    #region Load API

    public async Task<LoadDto> GetLoadAsync(string id)
    {
        var result = await GetRequestAsync<DataResult<LoadDto>>($"load/{id}");
        return result.Value!;
    }

    public async Task<PagedDataResult<LoadDto>> GetLoadsAsync(string searchInput = "", int page = 1, int pageSize = 10)
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
        var result = await GetRequestAsync<PagedDataResult<LoadDto>>("load/list", query);
        return result;
    }

    public Task CreateLoadAsync(LoadDto load)
    {
        return PostRequestAsync("load/create", load);
    }
    
    public Task UpdateLoadAsync(LoadDto load)
    {
        return PutRequestAsync($"load/update/{load.Id}", load);
    }

    public Task DeleteLoadAsync(string id)
    {
        return DeleteRequestAsync($"load/delete/{id}");
    }

    #endregion


    #region Truck API

    public async Task<TruckDto> GetTruckAsync(string id)
    {
        var result = await GetRequestAsync<DataResult<TruckDto>>($"truck/{id}");
        return result.Value!;
    }

    public async Task<TruckDto> GetTruckByDriverAsync(string driverId)
    {
        var result = await GetRequestAsync<DataResult<TruckDto>>($"truck/driver/{driverId}");
        return result.Value!;
    }

    public async Task<PagedDataResult<TruckDto>> GetTrucksAsync(string searchInput = "", int page = 1, int pageSize = 10, bool includeCargoIds = false)
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
        var result = await GetRequestAsync<PagedDataResult<TruckDto>>("truck/list", query);
        return result;
    }

    public Task CreateTruckAsync(TruckDto truck)
    {
        return PostRequestAsync("truck/create", truck);
    }

    public Task UpdateTruckAsync(TruckDto truck)
    {
        return PutRequestAsync($"truck/update/{truck.Id}", truck);
    }

    public Task DeleteTruckAsync(string id)
    {
        return DeleteRequestAsync($"truck/delete/{id}");
    }

    #endregion


    #region Employee API

    public async Task<EmployeeDto> GetEmployeeAsync(string id)
    {
        var result = await GetRequestAsync<DataResult<EmployeeDto>>($"employee/{id}");
        return result.Value!;
    }

    public async Task<PagedDataResult<EmployeeDto>> GetEmployeesAsync(string searchInput = "", int page = 1, int pageSize = 10)
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
        var result = await GetRequestAsync<PagedDataResult<EmployeeDto>>("employee/list", query);
        return result;
    }

    public async Task<bool> EmployeeExistsAsync(string id)
    {
        try
        {
            await GetEmployeeAsync(id);
            return true;
        }
        catch (ApiException)
        {
            return false;
        }
    }

    public Task CreateEmployeeAsync(EmployeeDto user)
    {
        return PostRequestAsync("employee/create", user);
    }

    public async Task<bool> TryCreateEmployeeAsync(EmployeeDto user)
    {
        if (string.IsNullOrEmpty(user.Id))
            throw new ApiException("Id is an empty");

        var userExists = await EmployeeExistsAsync(user.Id);

        if (!userExists)
        {
            await CreateEmployeeAsync(user);
            return true;
        }

        return false;
    }

    public Task UpdateEmployeeAsync(EmployeeDto user)
    {
        return PutRequestAsync($"employee/update/{user.Id}", user);
    }
    
    public Task DeleteEmployeeAsync(string id)
    {
        return DeleteRequestAsync($"employee/delete/{id}");
    }

    #endregion


    #region Tenant API

    public async Task<string> GetTenantDisplayNameAsync(string identifier)
    {
        var result = await GetRequestAsync<DataResult<string>>($"tenant/displayName/{identifier}");
        return result.Value!;
    }

    public async Task<TenantDto> GetTenantAsync(string identifier)
    {
        var result = await GetRequestAsync<DataResult<TenantDto>>($"tenant/{identifier}");
        return result.Value!;
    }

    public async Task<PagedDataResult<TenantDto>> GetTenantsAsync(string searchInput = "", int page = 1, int pageSize = 10)
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
        var result = await GetRequestAsync<PagedDataResult<TenantDto>>("tenant/list", query);
        return result;
    }

    public Task CreateTenantAsync(TenantDto tenant)
    {
        return PostRequestAsync("tenant/create", tenant);
    }

    public Task UpdateTenantAsync(TenantDto tenant)
    {
        return PutRequestAsync($"tenant/update/{tenant.Id}", tenant);
    }

    public Task DeleteTenantAsync(string id)
    {
        return DeleteRequestAsync($"tenant/delete/{id}");
    }

    #endregion

}

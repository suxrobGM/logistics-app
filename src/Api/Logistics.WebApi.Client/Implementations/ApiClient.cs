namespace Logistics.WebApi.Client.Implementations;

internal class ApiClient : ApiClientBase, IApiClient
{
    public ApiClient(ApiClientOptions options) : base(options.Host!)
    {
        SetCurrentTenantId(options.TenantId);
    }

    public string? CurrentTenantId { get; private set; }

    public void SetCurrentTenantId(string? tenantId)
    {
        if (CurrentTenantId == tenantId)
            return;
        
        CurrentTenantId = tenantId;
        SetRequestHeader("X-Tenant", tenantId);
    }

    #region Cargo API

    public async Task<CargoDto> GetCargoAsync(string id)
    {
        var result = await GetRequestAsync<DataResult<CargoDto>>($"cargo/{id}");
        return result.Value!;
    }

    public async Task<PagedDataResult<CargoDto>> GetCargoesAsync(string searchInput = "", int page = 1, int pageSize = 10)
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
        var result = await GetRequestAsync<PagedDataResult<CargoDto>>("cargo/list", query);
        return result;
    }

    public Task CreateCargoAsync(CargoDto cargo)
    {
        return PostRequestAsync("cargo/create", cargo);
    }
    
    public Task UpdateCargoAsync(CargoDto cargo)
    {
        return PutRequestAsync($"cargo/update/{cargo.Id}", cargo);
    }

    public Task DeleteCargoAsync(string id)
    {
        return DeleteRequestAsync($"cargo/{id}");
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
        return DeleteRequestAsync($"truck/{id}");
    }

    #endregion


    #region User API

    public async Task<UserDto> GetUserAsync(string id)
    {
        var result = await GetRequestAsync<DataResult<UserDto>>($"user/{id}");
        return result.Value!;
    }

    public async Task<PagedDataResult<UserDto>> GetUsersAsync(string searchInput = "", int page = 1, int pageSize = 10)
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
        var result = await GetRequestAsync<PagedDataResult<UserDto>>("user/list", query);
        return result;
    }

    public async Task<bool> UserExistsAsync(string externalId)
    {
        try
        {
            var result = await GetUserAsync(externalId);
            return result != null;
        }
        catch (ApiException)
        {
            return false;
        }
    }

    public Task CreateUserAsync(UserDto user)
    {
        return PostRequestAsync("user/create", user);
    }

    public async Task<bool> TryCreateUserAsync(UserDto user)
    {
        if (string.IsNullOrEmpty(user.ExternalId))
        {
            throw new ApiException("ExternalId is null or empty");
        }

        var userExists = await UserExistsAsync(user.ExternalId);

        if (!userExists)
        {
            await CreateUserAsync(user);
            return true;
        }

        return false;
    }

    public Task UpdateUserAsync(UserDto user)
    {
        return PutRequestAsync($"user/update/{user.Id}", user);
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
        return DeleteRequestAsync($"tenant/{id}");
    }

    #endregion
}

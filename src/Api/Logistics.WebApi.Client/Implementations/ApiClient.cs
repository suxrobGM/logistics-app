namespace Logistics.WebApi.Client;

internal class ApiClient : ApiClientBase, IApiClient
{
    public ApiClient(ApiClientOptions options) : base(options.Host!)
    {
    }


    #region Cargo API

    public Task<CargoDto> GetCargoAsync(string id)
    {
        return GetRequestAsync<CargoDto>($"api/cargo/{id}");
    }

    public Task CreateCargoAsync(CargoDto cargo)
    {
        return PostRequestAsync("api/cargo/create", cargo);
    }

    public async Task<PagedDataResult<CargoDto>> GetCargoesAsync(int page = 1, int pageSize = 10)
    {
        var query = new Dictionary<string, string>
        {
            {"page", page.ToString() },
            {"pageSize", pageSize.ToString() }
        };
        var result = await GetRequestAsync<PagedDataResult<CargoDto>>("api/cargo/list", query);
        return result;
    }

    public Task UpdateCargoAsync(CargoDto cargo)
    {
        return PutRequestAsync($"api/cargo/update/{cargo.Id}", cargo);
    }

    #endregion


    #region Truck API

    public Task<TruckDto> GetTruckAsync(string id)
    {
        return GetRequestAsync<TruckDto>($"api/truck/{id}");
    }

    public Task CreateTruckAsync(TruckDto truck)
    {
        return PostRequestAsync("api/truck/create", truck);
    }

    public async Task<PagedDataResult<TruckDto>> GetTrucksAsync(int page = 1, int pageSize = 10)
    {
        var query = new Dictionary<string, string>
        {
            {"page", page.ToString() },
            {"pageSize", pageSize.ToString() }
        };
        var result = await GetRequestAsync<PagedDataResult<TruckDto>>("api/truck/list", query);
        return result;
    }

    public Task UpdateTruckAsync(TruckDto truck)
    {
        return PutRequestAsync($"api/truck/update/{truck.Id}", truck);
    }

    #endregion


    #region User API

    public Task<UserDto> GetUserAsync(string id)
    {
        return GetRequestAsync<UserDto>($"api/user/{id}");
    }

    public async Task<PagedDataResult<UserDto>> GetUsersAsync(int page = 1, int pageSize = 10)
    {
        var query = new Dictionary<string, string>
        {
            {"page", page.ToString() },
            {"pageSize", pageSize.ToString() }
        };
        var result = await GetRequestAsync<PagedDataResult<UserDto>>("api/user/list", query);
        return result;
    }

    public Task CreateUserAsync(UserDto user)
    {
        return PostRequestAsync("api/user/create", user);
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

    public async Task<bool> UserExistsAsync(string externalId)
    {
        var query = new Dictionary<string, string>
        {
            {"externalId", externalId }
        };
        var result = await GetRequestAsync<DataResult<bool>>("api/user/exists", query);
        return result.Value;
    }

    public Task UpdateUserAsync(UserDto user)
    {
        return PutRequestAsync($"api/user/update/{user.Id}", user);
    }

    #endregion
}

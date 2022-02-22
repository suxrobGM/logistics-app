namespace Logistics.WebApi.Client;

internal class ApiClient : ApiClientBase, IApiClient
{
    public ApiClient(ApiClientOptions options) : base(options.Host!)
    {
    }

    public Task CreateUserAsync(UserDto userDto)
    {
        return PostRequestAsync("api/user", userDto);
    }

    public async Task<bool> TryCreateUserAsync(UserDto userDto)
    {
        if (string.IsNullOrEmpty(userDto.ExternalId))
        {
            throw new ApiException("ExternalId is null or empty");
        }

        var userExists = await UserExistsAsync(userDto.ExternalId);

        if (!userExists)
        {
            await CreateUserAsync(userDto);
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
}

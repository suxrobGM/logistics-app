using Logistics.Application.Contracts.Commands;

namespace Logistics.WebApi.Client;

internal class ApiClient : ApiClientBase, IApiClient
{
    public ApiClient(ApiClientOptions options) : base(options.Host!)
    {
    }

    public Task CreateUser(CreateUserCommand request)
    {
        return PostRequestAsync("api/user", CreateUser);
    }
}

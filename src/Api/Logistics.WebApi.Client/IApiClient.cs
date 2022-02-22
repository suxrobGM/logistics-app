using Logistics.Application.Contracts.Commands;

namespace Logistics.WebApi.Client;

public interface IApiClient
{
    Task CreateUser(CreateUserCommand request);
}

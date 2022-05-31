using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Logistics.Application.Handlers.Commands;

internal class AddUserRoleClaimsCommandHandler : 
    RequestHandlerBase<AddUserRoleClaimsCommand, DataResult<AzureConnectorResponse>>
{
    private readonly ITenantRepository<User> _repository;
    private readonly ILogger<AddUserRoleClaimsCommandHandler> _logger;

    public AddUserRoleClaimsCommandHandler(
        ITenantRepository<User> repository,
        ILogger<AddUserRoleClaimsCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    protected override async Task<DataResult<AzureConnectorResponse>> 
        HandleValidated(AddUserRoleClaimsCommand request, CancellationToken cancellationToken)
    {
        var clientId = request.AzureAdClientId;
        if (!clientId.Equals(request.ConnectorRequest.ClientId))
        {
            _logger.LogWarning("AzureAd ClientId is not authorized");
            return DataResult<AzureConnectorResponse>.CreateError("AzureAd ClientId is not authorized");
        }

        //// If email claim not found, show block page. Email is required and sent by default.
        //if (requestConnector.Email == null || requestConnector.Email == "" || requestConnector.Email.Contains("@") == false)
        //{
        //    return BadRequest(new ResponseContent("ShowBlockPage", "Email name is mandatory."));
        //}

        var result = new AzureConnectorResponse
        {
            UserRole = "everything awesome"
        };

        return DataResult<AzureConnectorResponse>.CreateSuccess(result);
    }

    protected override bool Validate(AddUserRoleClaimsCommand request, out string errorDescription)
    {
        errorDescription = string.Empty;
        var auth = request.AuthorizationHeader;
        const string username = "Logistics";
        const string password = "Test12345";

        if (!auth.StartsWith("Basic "))
        {
            _logger.LogWarning("Azure Api Connector handler: HTTP basic authentication header must start with 'Basic '.");
            errorDescription = "Azure Api Connector handler: HTTP basic authentication header must start with 'Basic '.";
            return false;
        }

        var cred = Encoding.UTF8.GetString(Convert.FromBase64String(auth[6..])).Split(':');
        return cred[0] == username && cred[1] == password;
    }
}

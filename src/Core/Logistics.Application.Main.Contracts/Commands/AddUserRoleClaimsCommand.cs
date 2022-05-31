namespace Logistics.Application.Contracts.Commands;

public class AddUserRoleClaimsCommand : RequestBase<DataResult<AzureConnectorResponse>>
{
    public AddUserRoleClaimsCommand(AzureConnectorRequest request, string authorizationHeader, string azureClientId)
    {
        ConnectorRequest = request;
        AuthorizationHeader = authorizationHeader;
        AzureAdClientId = azureClientId;
    }

    public AzureConnectorRequest ConnectorRequest { get; set; }
    public string AuthorizationHeader { get; set; }
    public string AzureAdClientId { get; set; }
}

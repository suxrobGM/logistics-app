using System.Text.Json.Serialization;

namespace Logistics.Application.Contracts.Models;

public class AzureConnectorResponse
{
    public const string ApiVersion = "1.0.0";

    public AzureConnectorResponse()
    {
        Version = ApiVersion;
        Action = "Continue";
    }

    public AzureConnectorResponse(string action, string userMessage)
    {
        Version = ApiVersion;
        Action = action;
        UserMessage = userMessage;
        if (action == "ValidationError")
        {
            Status = "400";
        }
    }

    [JsonPropertyName("version")]
    public string Version { get; }

    [JsonPropertyName("action")]
    public string Action { get; set; }

    [JsonPropertyName("userMessage")]
    public string? UserMessage { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("extension_UserRole")]
    public string UserRole { get; set; } = string.Empty;
}

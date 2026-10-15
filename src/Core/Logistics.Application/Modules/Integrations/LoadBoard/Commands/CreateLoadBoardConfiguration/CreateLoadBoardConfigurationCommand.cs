using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

[RequiresFeature(TenantFeature.LoadBoard)]
public class CreateLoadBoardConfigurationCommand : ICommand
{
    public LoadBoardProviderType ProviderType { get; set; }

    /// <summary>
    /// API key, or the login (DAT: service account email).
    /// </summary>
    public required string ApiKey { get; set; }

    /// <summary>
    /// API secret, or the password (DAT: service account password).
    /// </summary>
    public string? ApiSecret { get; set; }

    /// <summary>
    /// DAT: the DAT One user the API acts as.
    /// </summary>
    public string? UserEmail { get; set; }

    public bool MarketRatesEnabled { get; set; }
    public string? CompanyDotNumber { get; set; }
    public string? CompanyMcNumber { get; set; }
}

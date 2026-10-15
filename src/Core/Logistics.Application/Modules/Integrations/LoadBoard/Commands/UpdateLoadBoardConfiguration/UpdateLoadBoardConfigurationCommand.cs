using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Modules.Integrations.LoadBoard.Commands;

/// <summary>
/// Edits a provider connection. A null field is left unchanged, so secrets never have to be
/// sent back; changing any credential re-tests the connection before it is saved.
/// </summary>
[RequiresFeature(TenantFeature.LoadBoard)]
public class UpdateLoadBoardConfigurationCommand : ICommand
{
    public Guid Id { get; set; }
    public string? ApiKey { get; set; }
    public string? ApiSecret { get; set; }
    public string? UserEmail { get; set; }
    public bool? IsActive { get; set; }
    public bool? MarketRatesEnabled { get; set; }
    public string? CompanyDotNumber { get; set; }
    public string? CompanyMcNumber { get; set; }
}

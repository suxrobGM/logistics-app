using Logistics.Application.Attributes;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Compliance.Eld.Commands;

/// <summary>
/// Creates every currently suggested driver and vehicle mapping for the provider in one go. Suggestions
/// are recomputed server-side, so the client cannot slip in a pairing that was never suggested.
/// </summary>
[RequiresFeature(TenantFeature.Eld)]
public class ApplyEldMappingSuggestionsCommand : ICommand<Result<EldMappingSuggestionsDto>>
{
    public Guid ProviderId { get; set; }
}

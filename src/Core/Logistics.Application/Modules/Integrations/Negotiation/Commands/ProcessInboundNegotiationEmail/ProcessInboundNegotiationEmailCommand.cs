using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.Negotiation.Commands;

/// <summary>
/// Files a broker reply against its negotiation thread. Runs in the tenant scope the reply token
/// resolved to. A failed result means "try again later", not "bad request": the webhook route maps
/// it to a 5xx so the provider retries.
/// </summary>
/// <remarks>
/// Deliberately carries no <c>[RequiresFeature]</c> attribute - the caller is an anonymous webhook,
/// so the feature is checked in the handler body where nothing can skip it.
/// </remarks>
public class ProcessInboundNegotiationEmailCommand : ICommand<Result>
{
    public required string ThreadToken { get; set; }
    public required string ProviderEmailId { get; set; }
    public required string From { get; set; }
    public string? Subject { get; set; }
    public string? MessageId { get; set; }
}

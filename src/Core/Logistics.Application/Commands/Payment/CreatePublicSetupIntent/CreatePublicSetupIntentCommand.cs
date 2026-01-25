using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Creates a Stripe SetupIntent for a public payment link (no authentication required).
/// The SetupIntent is created on the connected account.
/// </summary>
public record CreatePublicSetupIntentCommand : IAppRequest<Result<SetupIntentDto>>
{
    public required Guid TenantId { get; set; }
    public required string Token { get; set; }
}

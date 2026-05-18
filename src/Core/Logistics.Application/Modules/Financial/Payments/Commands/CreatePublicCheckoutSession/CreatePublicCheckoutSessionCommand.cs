using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

/// <summary>
/// Creates a Stripe Checkout Session for a public invoice payment link. Returns the hosted-page
/// URL the customer is redirected to. Settlement is recorded by the
/// <c>checkout.session.completed</c> webhook.
/// </summary>
public record CreatePublicCheckoutSessionCommand : ICommand<Result<PublicCheckoutSessionDto>>
{
    public required Guid TenantId { get; init; }
    public required string Token { get; init; }

    /// <summary>URL Stripe redirects to on successful payment.</summary>
    public required string SuccessUrl { get; init; }

    /// <summary>URL Stripe redirects to when the customer cancels.</summary>
    public required string CancelUrl { get; init; }

    /// <summary>Amount to pay. If null, the full <c>amountDue</c> is charged.</summary>
    public decimal? Amount { get; init; }
}

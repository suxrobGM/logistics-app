using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Abstractions.Payments;

/// <summary>
/// Inputs for <see cref="IStripeConnectService.CreateConnectedCheckoutSessionAsync"/>.
/// </summary>
public sealed record CheckoutSessionRequest
{
    public required string ConnectedAccountId { get; init; }
    public required Money Amount { get; init; }
    public required string SuccessUrl { get; init; }
    public required string CancelUrl { get; init; }
    public required string LineItemDescription { get; init; }

    /// <summary>
    /// Optional pre-fill for the customer's email on the hosted page.
    /// </summary>
    public string? CustomerEmail { get; init; }

    /// <summary>
    /// Metadata copied verbatim onto the Stripe Session — used by the webhook to resolve
    /// the local entities (tenant, invoice) when the session completes.
    /// </summary>
    public IDictionary<string, string>? Metadata { get; init; }

    /// <summary>
    /// Platform fee as a percentage of <see cref="Amount"/>. Default is 0.
    /// </summary>
    public decimal ApplicationFeePercent { get; init; }
}

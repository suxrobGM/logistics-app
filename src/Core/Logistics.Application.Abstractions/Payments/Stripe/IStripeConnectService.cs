using Logistics.Domain.Entities;
using Address = Logistics.Domain.Primitives.ValueObjects.Address;

namespace Logistics.Application.Abstractions.Payments.Stripe;

/// <summary>
/// Stripe Connect operations - onboarding, payouts, and connected account payments.
/// Every method throws <see cref="StripeApiException"/> when Stripe rejects the request.
/// </summary>
public interface IStripeConnectService
{
    /// <summary>
    /// Creates a new Stripe Connect Express account for the tenant.
    /// </summary>
    /// <returns>The connected account ID.</returns>
    Task<string> CreateConnectedAccountAsync(Tenant tenant);

    /// <summary>
    /// Creates a new Stripe Connect Express account for an employee (individual) to receive payroll payouts.
    /// The connected account country is taken from <paramref name="employee"/>.Address when set,
    /// otherwise from <paramref name="fallbackAddress"/> (typically the tenant's company address).
    /// </summary>
    /// <returns>The connected account ID.</returns>
    Task<string> CreateEmployeeConnectedAccountAsync(Employee employee, Address fallbackAddress);

    /// <summary>
    /// Creates an account link for the employee's onboarding flow.
    /// </summary>
    Task<StripeAccountLink> CreateEmployeeAccountLinkAsync(
        string stripeConnectedAccountId, string returnUrl, string refreshUrl);

    /// <summary>
    /// Creates an account link for the onboarding flow.
    /// The user will be redirected to this URL to complete their Stripe onboarding.
    /// </summary>
    /// <param name="tenant">Tenant with an existing connected account.</param>
    /// <param name="returnUrl">URL to redirect to after successful onboarding.</param>
    /// <param name="refreshUrl">URL to redirect to if the link expires or needs refresh.</param>
    Task<StripeAccountLink> CreateAccountLinkAsync(Tenant tenant, string returnUrl, string refreshUrl);

    /// <summary>
    /// Updates the tenant's Stripe Connect status based on the account capabilities.
    /// Call this after receiving account.updated webhooks.
    /// </summary>
    Task SyncConnectedAccountStatusAsync(Tenant tenant);

    /// <summary>
    /// Creates a Stripe Checkout Session for a one-time payment that settles to a connected
    /// account via destination charges. The hosted page handles 3DS, SEPA mandates, etc.
    /// </summary>
    Task<StripeCheckoutLink> CreateConnectedCheckoutSessionAsync(CheckoutSessionRequest request);

    /// <summary>
    /// Creates a transfer to move funds from the platform account to a connected account.
    /// Use this for payouts or manual fund transfers.
    /// </summary>
    /// <param name="amount">Amount to transfer in major units; converted to the currency's minor units here.</param>
    /// <param name="currency">Currency code (e.g., "usd").</param>
    /// <param name="connectedAccountId">Destination connected account ID.</param>
    /// <param name="description">Optional description for the transfer.</param>
    /// <returns>The transfer ID.</returns>
    Task<string> CreateTransferAsync(
        decimal amount,
        string currency,
        string connectedAccountId,
        string? description = null);

    /// <summary>
    /// Creates a login link for the connected account's Stripe Express dashboard.
    /// </summary>
    /// <returns>The dashboard URL.</returns>
    Task<string> CreateLoginLinkAsync(string connectedAccountId);
}

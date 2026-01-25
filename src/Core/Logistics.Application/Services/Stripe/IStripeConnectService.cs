using Logistics.Domain.Entities;
using Stripe;

namespace Logistics.Application.Services;

/// <summary>
/// Service for Stripe Connect operations - onboarding, payouts, and connected account payments.
/// Separated from IStripeService to keep concerns isolated and allow future refactoring.
/// </summary>
public interface IStripeConnectService
{
    /// <summary>
    /// Creates a new Stripe Connect Express account for the tenant.
    /// </summary>
    /// <param name="tenant">Tenant to create the connected account for.</param>
    /// <returns>The created Stripe Account.</returns>
    Task<Account> CreateConnectedAccountAsync(Tenant tenant);

    /// <summary>
    /// Creates an account link for the onboarding flow.
    /// The user will be redirected to this URL to complete their Stripe onboarding.
    /// </summary>
    /// <param name="tenant">Tenant with an existing connected account.</param>
    /// <param name="returnUrl">URL to redirect to after successful onboarding.</param>
    /// <param name="refreshUrl">URL to redirect to if the link expires or needs refresh.</param>
    /// <returns>Account link with URL for onboarding.</returns>
    Task<AccountLink> CreateAccountLinkAsync(Tenant tenant, string returnUrl, string refreshUrl);

    /// <summary>
    /// Retrieves the connected account details from Stripe.
    /// </summary>
    /// <param name="accountId">Stripe connected account ID.</param>
    /// <returns>The Stripe Account object.</returns>
    Task<Account> GetConnectedAccountAsync(string accountId);

    /// <summary>
    /// Updates the tenant's Stripe Connect status based on the account capabilities.
    /// Call this after receiving account.updated webhooks.
    /// </summary>
    /// <param name="tenant">Tenant to update.</param>
    /// <returns>The updated account details.</returns>
    Task<Account> SyncConnectedAccountStatusAsync(Tenant tenant);

    /// <summary>
    /// Creates a PaymentIntent with destination charges to route payment to the connected account.
    /// </summary>
    /// <param name="payment">Payment entity with amount details.</param>
    /// <param name="connectedAccountId">The connected account to receive the payment.</param>
    /// <param name="applicationFeePercent">Optional platform fee percentage (0-100). Default is 0.</param>
    /// <returns>The created PaymentIntent.</returns>
    Task<PaymentIntent> CreateConnectedPaymentIntentAsync(
        Payment payment,
        string connectedAccountId,
        decimal applicationFeePercent = 0);

    /// <summary>
    /// Creates a SetupIntent for saving a payment method for future connected account payments.
    /// </summary>
    /// <param name="connectedAccountId">The connected account ID.</param>
    /// <returns>The SetupIntent with client secret for frontend use.</returns>
    Task<SetupIntent> CreateConnectedSetupIntentAsync(string connectedAccountId);

    /// <summary>
    /// Creates a transfer to move funds from the platform account to a connected account.
    /// Use this for payouts or manual fund transfers.
    /// </summary>
    /// <param name="amount">Amount to transfer in the smallest currency unit (e.g., cents).</param>
    /// <param name="currency">Currency code (e.g., "usd").</param>
    /// <param name="connectedAccountId">Destination connected account ID.</param>
    /// <param name="description">Optional description for the transfer.</param>
    /// <returns>The created Transfer object.</returns>
    Task<Transfer> CreateTransferAsync(
        long amount,
        string currency,
        string connectedAccountId,
        string? description = null);

    /// <summary>
    /// Retrieves the login link for the connected account's Stripe Express dashboard.
    /// </summary>
    /// <param name="connectedAccountId">The connected account ID.</param>
    /// <returns>Login link with URL to the Express dashboard.</returns>
    Task<LoginLink> CreateLoginLinkAsync(string connectedAccountId);
}

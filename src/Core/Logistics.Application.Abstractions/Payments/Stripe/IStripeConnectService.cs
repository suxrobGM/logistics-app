using Logistics.Domain.Entities;
using Stripe;
using Stripe.Checkout;
using Logistics.Application.Abstractions.Payments.Stripe;
using Logistics.Application.Abstractions.Payments;
using Address = Logistics.Domain.Primitives.ValueObjects.Address;

namespace Logistics.Application.Abstractions.Payments.Stripe;

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
    /// Creates a new Stripe Connect Express account for an employee (individual) to receive payroll payouts.
    /// The connected account country is taken from <paramref name="employee"/>.Address when set,
    /// otherwise from <paramref name="fallbackAddress"/> (typically the tenant's company address).
    /// </summary>
    Task<Account> CreateEmployeeConnectedAccountAsync(Employee employee, Address fallbackAddress);

    /// <summary>
    /// Creates an account link for the employee's onboarding flow.
    /// </summary>
    Task<AccountLink> CreateEmployeeAccountLinkAsync(
        string stripeConnectedAccountId, string returnUrl, string refreshUrl);

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
    /// Creates a Stripe Checkout Session for a one-time payment that settles to a connected
    /// account via destination charges. The hosted page handles 3DS, SEPA mandates, etc.
    /// </summary>
    Task<Session> CreateConnectedCheckoutSessionAsync(CheckoutSessionRequest request);

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

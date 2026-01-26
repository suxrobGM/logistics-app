using Logistics.Application.Services;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Geo;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Stripe;
using Address = Logistics.Domain.Primitives.ValueObjects.Address;

namespace Logistics.Infrastructure.Payments.Stripe;

/// <summary>
///     Stripe Connect service implementation for handling connected accounts and destination charges.
/// </summary>
internal class StripeConnectService : IStripeConnectService
{
    private readonly ILogger<StripeConnectService> logger;

    public StripeConnectService(IOptions<StripeOptions> options, ILogger<StripeConnectService> logger)
    {
        this.logger = logger;
        StripeConfiguration.ApiKey = options.Value.SecretKey;
    }

    public async Task<Account> CreateConnectedAccountAsync(Tenant tenant)
    {
        var options = new AccountCreateOptions
        {
            Type = "express",
            Country = GetCountryFromAddress(tenant.CompanyAddress),
            Email = tenant.BillingEmail,
            BusinessType = "company",
            Company =
                new AccountCompanyOptions
                {
                    Name = tenant.CompanyName ?? tenant.Name,
                    Address = tenant.CompanyAddress.ToStripeAddressOptions(),
                    Phone = tenant.PhoneNumber
                },
            Capabilities = new AccountCapabilitiesOptions
            {
                CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true },
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                UsBankAccountAchPayments =
                    new AccountCapabilitiesUsBankAccountAchPaymentsOptions { Requested = true }
            },
            Metadata = new Dictionary<string, string> { [StripeMetadataKeys.TenantId] = tenant.Id.ToString() }
        };

        var account = await new AccountService().CreateAsync(options);
        logger.LogInformation(
            "Created Stripe Connect account {AccountId} for tenant {TenantId}",
            account.Id, tenant.Id);

        return account;
    }

    public async Task<AccountLink> CreateAccountLinkAsync(Tenant tenant, string returnUrl, string refreshUrl)
    {
        if (string.IsNullOrEmpty(tenant.StripeConnectedAccountId))
        {
            throw new ArgumentException("Tenant must have a StripeConnectedAccountId");
        }

        var options = new AccountLinkCreateOptions
        {
            Account = tenant.StripeConnectedAccountId,
            RefreshUrl = refreshUrl,
            ReturnUrl = returnUrl,
            Type = "account_onboarding"
        };

        var accountLink = await new AccountLinkService().CreateAsync(options);
        logger.LogInformation(
            "Created account link for tenant {TenantId}, expires at {ExpiresAt}, link: {Link}",
            tenant.Id, accountLink.ExpiresAt, accountLink.Url);

        return accountLink;
    }

    public async Task<Account> GetConnectedAccountAsync(string accountId) =>
        await new AccountService().GetAsync(accountId);

    public async Task<Account> SyncConnectedAccountStatusAsync(Tenant tenant)
    {
        if (string.IsNullOrEmpty(tenant.StripeConnectedAccountId))
        {
            throw new ArgumentException("Tenant must have a StripeConnectedAccountId");
        }

        var account = await GetConnectedAccountAsync(tenant.StripeConnectedAccountId);

        // Update tenant's connect status based on account capabilities
        tenant.ChargesEnabled = account.ChargesEnabled;
        tenant.PayoutsEnabled = account.PayoutsEnabled;
        tenant.ConnectStatus = DetermineConnectStatus(account);

        logger.LogInformation(
            "Synced Connect status for tenant {TenantId}: Status={Status}, ChargesEnabled={ChargesEnabled}, PayoutsEnabled={PayoutsEnabled}",
            tenant.Id, tenant.ConnectStatus, tenant.ChargesEnabled, tenant.PayoutsEnabled);

        return account;
    }

    public async Task<PaymentIntent> CreateConnectedPaymentIntentAsync(
        Payment payment,
        string connectedAccountId,
        decimal applicationFeePercent = 0)
    {
        var amountInCents = (long)(payment.Amount.Amount * 100);

        var options = new PaymentIntentCreateOptions
        {
            Amount = amountInCents,
            Currency = payment.Amount.Currency.ToLower(),
            AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions { Enabled = true },
            Description = payment.Description,
            Metadata = new Dictionary<string, string> { ["PaymentId"] = payment.Id.ToString() },
            // Use destination charges to route payment to the connected account
            TransferData = new PaymentIntentTransferDataOptions { Destination = connectedAccountId }
        };

        // Calculate and set application fee if specified
        if (applicationFeePercent > 0)
        {
            var feeAmount = (long)(amountInCents * (applicationFeePercent / 100));
            options.ApplicationFeeAmount = feeAmount;
        }

        var paymentIntent = await new PaymentIntentService().CreateAsync(options);
        logger.LogInformation(
            "Created connected PaymentIntent {PaymentIntentId} for amount {Amount} to account {AccountId}",
            paymentIntent.Id, payment.Amount.Amount, connectedAccountId);

        return paymentIntent;
    }

    public async Task<SetupIntent> CreateConnectedSetupIntentAsync(string connectedAccountId)
    {
        var options = new SetupIntentCreateOptions
        {
            AutomaticPaymentMethods = new SetupIntentAutomaticPaymentMethodsOptions { Enabled = true },
            OnBehalfOf = connectedAccountId,
            Metadata = new Dictionary<string, string> { ["ConnectedAccountId"] = connectedAccountId }
        };

        var setupIntent = await new SetupIntentService().CreateAsync(options);
        logger.LogInformation(
            "Created connected SetupIntent {SetupIntentId} for account {AccountId}",
            setupIntent.Id, connectedAccountId);

        return setupIntent;
    }

    public async Task<Transfer> CreateTransferAsync(
        long amount,
        string currency,
        string connectedAccountId,
        string? description = null)
    {
        var options = new TransferCreateOptions
        {
            Amount = amount,
            Currency = currency.ToLower(),
            Destination = connectedAccountId,
            Description = description
        };

        var transfer = await new TransferService().CreateAsync(options);
        logger.LogInformation(
            "Created transfer {TransferId} for amount {Amount} {Currency} to account {AccountId}",
            transfer.Id, amount, currency, connectedAccountId);

        return transfer;
    }

    public async Task<LoginLink> CreateLoginLinkAsync(string connectedAccountId)
    {
        var loginLink = await new AccountLoginLinkService().CreateAsync(connectedAccountId);
        logger.LogInformation("Created login link for connected account {AccountId}", connectedAccountId);
        return loginLink;
    }

    #region Helpers

    private static string GetCountryFromAddress(Address address)
    {
        if (string.IsNullOrEmpty(address.Country))
        {
            return "US";
        }

        // If already a 2-letter code, return as-is
        if (address.Country.Length == 2)
        {
            return address.Country.ToUpperInvariant();
        }

        // Look up the country by name and return the ISO code
        var country = Countries.FindCountry(address.Country);
        return country?.Code ?? "US";
    }

    private static StripeConnectStatus DetermineConnectStatus(Account account)
    {
        if (!account.DetailsSubmitted)
        {
            return StripeConnectStatus.Pending;
        }

        if (account.Requirements?.DisabledReason is not null)
        {
            return account.Requirements.DisabledReason switch
            {
                "requirements.past_due" or "requirements.pending_verification" => StripeConnectStatus.Restricted,
                "rejected.fraud" or "rejected.terms_of_service" or "rejected.listed" or "rejected.other" =>
                    StripeConnectStatus.Disabled,
                _ => StripeConnectStatus.Restricted
            };
        }

        if (account.ChargesEnabled && account.PayoutsEnabled)
        {
            return StripeConnectStatus.Active;
        }

        return StripeConnectStatus.Restricted;
    }

    #endregion
}

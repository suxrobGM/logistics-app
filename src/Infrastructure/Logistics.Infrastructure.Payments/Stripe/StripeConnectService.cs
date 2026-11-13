using Logistics.Application.Abstractions.Payments;
using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.Geo;
using Microsoft.Extensions.Logging;
using Stripe;
using Stripe.Checkout;
using Address = Logistics.Domain.Primitives.ValueObjects.Address;

namespace Logistics.Infrastructure.Payments.Stripe;

/// <summary>
///     Stripe Connect service implementation for handling connected accounts and destination charges.
/// </summary>
internal sealed class StripeConnectService(ILogger<StripeConnectService> logger) : IStripeConnectService
{
    public async Task<string> CreateConnectedAccountAsync(Tenant tenant)
    {
        var country = GetCountryFromAddress(tenant.CompanyAddress);
        var options = new AccountCreateOptions
        {
            Type = "express",
            Country = country,
            Email = tenant.BillingEmail,
            BusinessType = "company",
            Company =
                new AccountCompanyOptions
                {
                    Name = tenant.DisplayName,
                    Address = tenant.CompanyAddress.ToStripeAddressOptions(),
                    Phone = tenant.PhoneNumber
                },
            Capabilities = StripeCapabilities.ForCountry(country),
            Metadata = new Dictionary<string, string> { [StripeMetadataKeys.TenantId] = tenant.Id.ToString() }
        };

        var account = await StripeApi.CallAsync(() => new AccountService().CreateAsync(options));
        logger.LogInformation(
            "Created Stripe Connect account {AccountId} for tenant {TenantId}",
            account.Id, tenant.Id);

        return account.Id;
    }

    public async Task<string> CreateEmployeeConnectedAccountAsync(Employee employee, Address fallbackAddress)
    {
        var address = employee.Address ?? fallbackAddress;
        var country = GetCountryFromAddress(address);

        var options = new AccountCreateOptions
        {
            Type = "express",
            Country = country,
            Email = employee.Email,
            BusinessType = "individual",
            Individual = new AccountIndividualOptions
            {
                FirstName = employee.FirstName,
                LastName = employee.LastName,
                Email = employee.Email,
                Phone = employee.PhoneNumber,
                Address = address.ToStripeAddressOptions()
            },
            Capabilities = new AccountCapabilitiesOptions
            {
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true }
            },
            Metadata = new Dictionary<string, string>
            {
                ["employee_id"] = employee.Id.ToString(),
                ["type"] = "employee_payout"
            }
        };

        var account = await StripeApi.CallAsync(() => new AccountService().CreateAsync(options));
        logger.LogInformation(
            "Created Stripe Connect account {AccountId} for employee {EmployeeId}",
            account.Id, employee.Id);

        return account.Id;
    }

    public async Task<StripeAccountLink> CreateEmployeeAccountLinkAsync(
        string stripeConnectedAccountId, string returnUrl, string refreshUrl)
    {
        var options = new AccountLinkCreateOptions
        {
            Account = stripeConnectedAccountId,
            RefreshUrl = refreshUrl,
            ReturnUrl = returnUrl,
            Type = "account_onboarding"
        };

        var accountLink = await StripeApi.CallAsync(() => new AccountLinkService().CreateAsync(options));
        logger.LogInformation(
            "Created account link for employee account {AccountId}",
            stripeConnectedAccountId);

        return new StripeAccountLink(accountLink.Url, accountLink.ExpiresAt);
    }

    public async Task<StripeAccountLink> CreateAccountLinkAsync(Tenant tenant, string returnUrl, string refreshUrl)
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

        var accountLink = await StripeApi.CallAsync(() => new AccountLinkService().CreateAsync(options));
        logger.LogInformation(
            "Created account link for tenant {TenantId}, expires at {ExpiresAt}, link: {Link}",
            tenant.Id, accountLink.ExpiresAt, accountLink.Url);

        return new StripeAccountLink(accountLink.Url, accountLink.ExpiresAt);
    }

    public async Task SyncConnectedAccountStatusAsync(Tenant tenant)
    {
        if (string.IsNullOrEmpty(tenant.StripeConnectedAccountId))
        {
            throw new ArgumentException("Tenant must have a StripeConnectedAccountId");
        }

        var accountId = tenant.StripeConnectedAccountId;
        var account = await StripeApi.CallAsync(() => new AccountService().GetAsync(accountId));

        // Update tenant's connect status based on account capabilities
        tenant.ChargesEnabled = account.ChargesEnabled;
        tenant.PayoutsEnabled = account.PayoutsEnabled;
        tenant.ConnectStatus = DetermineConnectStatus(account);

        logger.LogInformation(
            "Synced Connect status for tenant {TenantId}: Status={Status}, ChargesEnabled={ChargesEnabled}, PayoutsEnabled={PayoutsEnabled}",
            tenant.Id, tenant.ConnectStatus, tenant.ChargesEnabled, tenant.PayoutsEnabled);
    }

    public async Task<StripeCheckoutLink> CreateConnectedCheckoutSessionAsync(CheckoutSessionRequest request)
    {
        var amountInCents = StripeMoneyConversion.ToMinorUnits(request.Amount.Amount, request.Amount.Currency);

        var paymentIntentData = new SessionPaymentIntentDataOptions
        {
            // Destination charges route the payment to the connected account.
            TransferData = new SessionPaymentIntentDataTransferDataOptions
            {
                Destination = request.ConnectedAccountId
            },
            Metadata = request.Metadata is null
                ? null
                : [with(request.Metadata)]
        };

        if (request.ApplicationFeePercent > 0)
        {
            paymentIntentData.ApplicationFeeAmount =
                (long)(amountInCents * (request.ApplicationFeePercent / 100));
        }

        var options = new SessionCreateOptions
        {
            Mode = "payment",
            SuccessUrl = request.SuccessUrl,
            CancelUrl = request.CancelUrl,
            CustomerEmail = request.CustomerEmail,
            // Payment methods are auto-selected from the connected account's capabilities
            // (see StripeCapabilities.ForCountry). No need to pass payment_method_types.
            LineItems =
            [
                new SessionLineItemOptions
                {
                    Quantity = 1,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = request.Amount.Currency.ToLower(),
                        UnitAmount = amountInCents,
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = request.LineItemDescription
                        }
                    }
                }
            ],
            PaymentIntentData = paymentIntentData,
            Metadata = request.Metadata is null
                ? null
                : [with(request.Metadata)]
        };

        var session = await StripeApi.CallAsync(() => new SessionService().CreateAsync(options));
        logger.LogInformation(
            "Created Checkout Session {SessionId} for amount {Amount} {Currency} to account {AccountId}",
            session.Id, request.Amount.Amount, request.Amount.Currency, request.ConnectedAccountId);

        return new StripeCheckoutLink(session.Id, session.Url);
    }

    public async Task<string> CreateTransferAsync(
        decimal amount,
        string currency,
        string connectedAccountId,
        string? description = null)
    {
        var options = new TransferCreateOptions
        {
            Amount = StripeMoneyConversion.ToMinorUnits(amount, currency),
            Currency = currency.ToLower(),
            Destination = connectedAccountId,
            Description = description
        };

        var transfer = await StripeApi.CallAsync(() => new TransferService().CreateAsync(options));
        logger.LogInformation(
            "Created transfer {TransferId} for amount {Amount} {Currency} to account {AccountId}",
            transfer.Id, amount, currency, connectedAccountId);

        return transfer.Id;
    }

    public async Task<string> CreateLoginLinkAsync(string connectedAccountId)
    {
        var loginLink = await StripeApi.CallAsync(() => new AccountLoginLinkService().CreateAsync(connectedAccountId));
        logger.LogInformation("Created login link for connected account {AccountId}", connectedAccountId);
        return loginLink.Url;
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

using Logistics.Domain.Entities;
using Logistics.Shared.Consts;
using Stripe;
using AddressValueObject = Logistics.Domain.ValueObjects.Address;
using PaymentMethod = Logistics.Domain.Entities.PaymentMethod;
using StripePaymentMethod = Stripe.PaymentMethod;
using StripeAddress = Stripe.Address;

namespace Logistics.Application.Services;

public static class StripeObjectMapper
{
    public static AddressOptions ToStripeAddressOptions(this AddressValueObject address)
    {
        // var country = Countries.FindCountry(address.Country) ??
        //               throw new InvalidOperationException($"Country {address.Country} not found");
        
        return new AddressOptions
        {
            City = address.City,
            Country = address.Country,
            Line1 = address.Line1,
            Line2 = address.Line2,
            PostalCode = address.ZipCode,
            State = address.State,
        };
    }
    
    public static AddressValueObject ToAddressEntity(this StripeAddress stripeAddress)
    {
        return new AddressValueObject
        {
            City = stripeAddress.City,
            Country = stripeAddress.Country,
            Line1 = stripeAddress.Line1,
            Line2 = stripeAddress.Line2,
            ZipCode = stripeAddress.PostalCode,
            State = stripeAddress.State,
        };
    }
    
    public static AddressValueObject ToAddressEntity(this AddressOptions addressOptions)
    {
        return new AddressValueObject
        {
            City = addressOptions.City,
            Country = addressOptions.Country,
            Line1 = addressOptions.Line1,
            Line2 = addressOptions.Line2,
            ZipCode = addressOptions.PostalCode,
            State = addressOptions.State,
        };
    }
    
    public static PaymentMethod ToPaymentMethodEntity(this StripePaymentMethod stripePaymentMethod)
    {
        if (stripePaymentMethod.Card is not null)
        {
            return new CardPaymentMethod
            {
                Type = PaymentMethodType.Card,
                CardHolderName = stripePaymentMethod.BillingDetails.Name,
                CardNumber = $"**** **** **** {stripePaymentMethod.Card.Last4}",
                Cvc = "***",
                ExpMonth = (int)stripePaymentMethod.Card.ExpMonth,
                ExpYear = (int)stripePaymentMethod.Card.ExpYear,
                BillingAddress = stripePaymentMethod.BillingDetails.Address.ToAddressEntity(),
                StripePaymentMethodId = stripePaymentMethod.Id,
                VerificationStatus = PaymentMethodVerificationStatus.Verified,
            };
        }
        
        if (stripePaymentMethod.UsBankAccount is not null)
        {
            return new UsBankAccountPaymentMethod
            {
                Type = PaymentMethodType.UsBankAccount,
                AccountHolderName = stripePaymentMethod.BillingDetails.Name,
                AccountNumber = $"********{stripePaymentMethod.UsBankAccount.Last4}",
                RoutingNumber = stripePaymentMethod.UsBankAccount.RoutingNumber,
                BankName = stripePaymentMethod.UsBankAccount.BankName,
                AccountHolderType = GetAccountHolderType(stripePaymentMethod.UsBankAccount.AccountHolderType),
                AccountType = GetAccountType(stripePaymentMethod.UsBankAccount.AccountType),
                BillingAddress = stripePaymentMethod.BillingDetails.Address.ToAddressEntity(),
                StripePaymentMethodId = stripePaymentMethod.Id,
                VerificationStatus = PaymentMethodVerificationStatus.Verified
            };
        }
        
        throw new NotSupportedException("Unsupported payment method type.");
    }

    public static UsBankAccountHolderType GetAccountHolderType(string accountHolderType)
    {
        return accountHolderType switch
        {
            "individual" => UsBankAccountHolderType.Individual,
            "company" => UsBankAccountHolderType.Business,
            _ => throw new ArgumentOutOfRangeException(nameof(accountHolderType), accountHolderType)
        };
    }

    public static UsBankAccountType GetAccountType(string accountType)
    {
        return accountType switch
        {
            "checking" => UsBankAccountType.Checking,
            "savings" => UsBankAccountType.Savings,
            _ => throw new ArgumentOutOfRangeException(nameof(accountType), accountType)
        };
    }

    public static SubscriptionStatus GetSubscriptionStatus(string stripeSubscriptionStatus)
    {
        return stripeSubscriptionStatus switch
        {
            "active" => SubscriptionStatus.Active,
            "past_due" => SubscriptionStatus.PastDue,
            "canceled" => SubscriptionStatus.Cancelled,
            "incomplete" => SubscriptionStatus.Incomplete,
            "incomplete_expired" => SubscriptionStatus.IncompleteExpired,
            "trialing" => SubscriptionStatus.Trialing,
            "unpaid" => SubscriptionStatus.Unpaid,
            "paused" => SubscriptionStatus.Paused,
            _ => throw new ArgumentOutOfRangeException(nameof(stripeSubscriptionStatus), stripeSubscriptionStatus)
        };
    }
}
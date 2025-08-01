using Logistics.Domain.Entities;
using Logistics.Domain.Primitives.Enums;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class PaymentMethodMapper
{
    public static PaymentMethodDto ToDto(this PaymentMethod entity)
    {
        return entity switch
        {
            CardPaymentMethod cardPaymentMethod => new PaymentMethodDto()
            {
                Id = cardPaymentMethod.Id,
                CardNumber = $"**** **** **** {cardPaymentMethod.CardNumber[^4..]}",
                Cvc = "***",
                ExpMonth = cardPaymentMethod.ExpMonth,
                ExpYear = cardPaymentMethod.ExpYear,
                IsDefault = cardPaymentMethod.IsDefault,
                Type = PaymentMethodType.Card,
                CardHolderName = cardPaymentMethod.CardHolderName,
                BillingAddress = cardPaymentMethod.BillingAddress,
                VerificationStatus = cardPaymentMethod.VerificationStatus
            },
            UsBankAccountPaymentMethod usBankAccountPaymentMethod => new PaymentMethodDto
            {
                Id = usBankAccountPaymentMethod.Id,
                AccountNumber = $"********{usBankAccountPaymentMethod.AccountNumber[^4..]}",
                BankName = usBankAccountPaymentMethod.BankName,
                RoutingNumber = usBankAccountPaymentMethod.RoutingNumber,
                AccountHolderType = usBankAccountPaymentMethod.AccountHolderType,
                AccountHolderName = usBankAccountPaymentMethod.AccountHolderName,
                AccountType = usBankAccountPaymentMethod.AccountType,
                IsDefault = usBankAccountPaymentMethod.IsDefault,
                Type = PaymentMethodType.UsBankAccount,
                BillingAddress = usBankAccountPaymentMethod.BillingAddress,
                VerificationStatus = usBankAccountPaymentMethod.VerificationStatus,
                VerificationUrl = usBankAccountPaymentMethod.VerificationUrl
            },
            BankAccountPaymentMethod bankAccountPaymentMethod => new PaymentMethodDto
            {
                Id = bankAccountPaymentMethod.Id,
                AccountNumber = $"********{bankAccountPaymentMethod.AccountNumber[^4..]}",
                BankName = bankAccountPaymentMethod.BankName,
                AccountHolderName = bankAccountPaymentMethod.AccountHolderName,
                SwiftCode = bankAccountPaymentMethod.SwiftCode,
                IsDefault = bankAccountPaymentMethod.IsDefault,
                Type = PaymentMethodType.InternationalBankAccount,
                BillingAddress = bankAccountPaymentMethod.BillingAddress,
                VerificationStatus = bankAccountPaymentMethod.VerificationStatus
            },
            _ => throw new NotImplementedException($"Mapping for {entity.GetType().Name} is not implemented.")
        };
    }
}

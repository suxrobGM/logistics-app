using Logistics.Domain.Entities;
using Logistics.Shared.Consts;
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
                CardBrand = cardPaymentMethod.CardBrand,
                CardNumber = $"**** **** **** {cardPaymentMethod.CardNumber[^4..]}",
                Cvc = "***",
                ExpMonth = cardPaymentMethod.ExpMonth,
                ExpYear = cardPaymentMethod.ExpYear,
                FundingType = cardPaymentMethod.FundingType,
                IsDefault = cardPaymentMethod.IsDefault,
                Type = PaymentMethodType.Card,
                CardHolderName = cardPaymentMethod.CardHolderName,
                BillingAddress = cardPaymentMethod.BillingAddress.ToDto()
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
                BillingAddress = usBankAccountPaymentMethod.BillingAddress.ToDto()
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
                BillingAddress = bankAccountPaymentMethod.BillingAddress.ToDto()
            },
            _ => throw new NotImplementedException($"Mapping for {entity.GetType().Name} is not implemented.")
        };
    }
}
using Logistics.Shared.Consts;

namespace Logistics.Domain.Entities;

public class CardPaymentMethod : PaymentMethod
{
    public required string CardHolderName { get; set; }
    public required string CardBrand { get; set; } // visa, mastercard, etc.
    public required string CardNumber { get; set; }
    public required string Cvc { get; set; }
    public required int ExpMonth { get; set; }
    public required int ExpYear { get; set; }
    public CardFundingType FundingType { get; set; }
}
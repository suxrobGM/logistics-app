using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;

namespace Logistics.Application.Commands;

public class ProcessPaymentCommand : IAppRequest
{
    public Guid PaymentId { get; set; }
    public PaymentMethodType PaymentMethod { get; set; }
    public string? CardholderName { get; set; }
    public string? CardNumber { get; set; }
    public string? CardExpirationDate { get; set; }
    public string? CardCvv { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankRoutingNumber { get; set; }
    public string BillingAddress { get; set; } = null!;
}

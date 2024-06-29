using Logistics.Shared.Consts;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class ProcessPaymentCommand : IRequest<Result>
{
    public string PaymentId { get; set; } = default!;
    public PaymentMethod PaymentMethod { get; set; }
    public string? CardholderName { get; set; }
    public string? CardNumber { get; set; }
    public string? CardExpirationDate { get; set; }
    public string? CardCvv { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankRoutingNumber { get; set; }
    public string BillingAddress { get; set; } = default!;
}

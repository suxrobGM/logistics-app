using Logistics.Application.Abstractions;
using Logistics.Domain.Primitives.Enums;
using Logistics.Domain.Primitives.ValueObjects;

namespace Logistics.Application.Modules.Financial.Payments.Commands;

public class UpdatePaymentCommand : ICommand
{
    public Guid Id { get; set; }
    public string? StripePaymentMethodId { get; set; }
    public decimal? Amount { get; set; }
    public PaymentStatus? Status { get; set; }
    public Address? BillingAddress { get; set; }
    public string? Description { get; set; }
}
